using System.Runtime.CompilerServices;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Taken from: https://github.com/krubbles/Icaria-Noise
    /// </summary>
    internal class HelperPerlinNoise
    {
        public static float GradientNoise(float x, float y, int seed = 0)
        {
            int ix = x > 0 ? (int)x : (int)x - 1;
            int iy = y > 0 ? (int)y : (int)y - 1;
            float fx = x - ix;
            float fy = y - iy;

            ix += Const.Offset;
            iy += Const.Offset;
            ix += Const.SeedPrime * seed; 
            int p1 = ix * Const.XPrime1 + iy * Const.YPrime1;
            int p2 = ix * Const.XPrime2 + iy * Const.YPrime2;
            int llHash = p1 * p2;
            int lrHash = (p1 + Const.XPrime1) * (p2 + Const.XPrime2);
            int ulHash = (p1 + Const.YPrime1) * (p2 + Const.YPrime2);
            int urHash = (p1 + Const.XPlusYPrime1) * (p2 + Const.XPlusYPrime2);
            return InterpolateGradients2D(llHash, lrHash, ulHash, urHash, fx, fy);
        }

        public static (float x, float y) GradientNoiseVec2(float x, float y, int seed = 0)
        {
            int ix = x > 0 ? (int)x : (int)x - 1;
            int iy = y > 0 ? (int)y : (int)y - 1;
            float fx = x - ix;
            float fy = y - iy;

            ix += Const.Offset;
            iy += Const.Offset;
            ix += Const.SeedPrime * seed; // add seed before hashing to propigate its effect
            int p1 = ix * Const.XPrime1 + iy * Const.YPrime1;
            int p2 = ix * Const.XPrime2 + iy * Const.YPrime2;
            int llHash = p1 * p2;
            int lrHash = (p1 + Const.XPrime1) * (p2 + Const.XPrime2);
            int ulHash = (p1 + Const.YPrime1) * (p2 + Const.YPrime2);
            int urHash = (p1 + Const.XPlusYPrime1) * (p2 + Const.XPlusYPrime2);

            x = InterpolateGradients2D(llHash, lrHash, ulHash, urHash, fx, fy);
            y = InterpolateGradients2D(
                llHash * Const.XPrime1, lrHash * Const.XPrime1,
                ulHash * Const.XPrime1, urHash * Const.XPrime1, fx, fy);
            return (x, y);
        }

        public static float GradientNoisePeriodic(float x, float y, in NoisePeriod period, int seed = 0)
        {
            int ix = x > 0 ? (int)x : (int)x - 1;
            int iy = y > 0 ? (int)y : (int)y - 1;
            float fx = x - ix;
            float fy = y - iy;

            seed *= Const.SeedPrime << Const.PeriodShift;
            ix += seed;
            iy += seed;

            int left = ix * period.xf;
            int lower = iy * period.yf;
            int right = left + period.xf;
            int upper = lower + period.yf;
            left >>= Const.PeriodShift;
            lower >>= Const.PeriodShift;
            right >>= Const.PeriodShift;
            upper >>= Const.PeriodShift;
            int llHash = Hash(left, lower);
            int lrHash = Hash(right, lower);
            int ulHash = Hash(left, upper);
            int urHash = Hash(right, upper);
            return InterpolateGradients2D(llHash, lrHash, ulHash, urHash, fx, fy);
        }

        public static (float x, float y) GradientNoisePeriodicVec2(float x, float y, in NoisePeriod period, int seed = 0)
        {
            int ix = x > 0 ? (int)x : (int)x - 1;
            int iy = y > 0 ? (int)y : (int)y - 1;
            float fx = x - ix;
            float fy = y - iy;

            seed *= Const.SeedPrime << Const.PeriodShift;
            ix += seed;
            iy += seed;

            int left = ix * period.xf; // left
            int lower = iy * period.yf; // lower
            int right = left + period.xf; // right
            int upper = lower + period.yf; // upper
            left >>= Const.PeriodShift;
            lower >>= Const.PeriodShift;
            right >>= Const.PeriodShift;
            upper >>= Const.PeriodShift;

            int llHash = Hash(left, lower);
            int lrHash = Hash(right, lower);
            int ulHash = Hash(left, upper);
            int urHash = Hash(right, upper);
            x = InterpolateGradients2D(llHash, lrHash, ulHash, urHash, fx, fy);
            y = InterpolateGradients2D(
                llHash * Const.XPrime1, lrHash * Const.XPrime1,
                ulHash * Const.XPrime1, urHash * Const.XPrime1, fx, fy);
            return (x, y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe float InterpolateGradients2D(int llHash, int lrHash, int ulHash, int urHash, float fx, float fy)
        {
            int xHash, yHash;
            xHash = (llHash & Const.GradAndMask) | Const.GradOrMask;
            yHash = xHash << Const.GradShift1;
            float llGrad = fx * *(float*)&xHash + fy * *(float*)&yHash; // dot-product
            xHash = (lrHash & Const.GradAndMask) | Const.GradOrMask;
            yHash = xHash << Const.GradShift1;
            float lrGrad = (fx - 1) * *(float*)&xHash + fy * *(float*)&yHash;
            xHash = (ulHash & Const.GradAndMask) | Const.GradOrMask;
            yHash = xHash << Const.GradShift1;
            float ulGrad = fx * *(float*)&xHash + (fy - 1) * *(float*)&yHash; // dot-product
            xHash = (urHash & Const.GradAndMask) | Const.GradOrMask;
            yHash = xHash << Const.GradShift1;
            float urGrad = (fx - 1) * *(float*)&xHash + (fy - 1) * *(float*)&yHash;

            float sx = fx * fx * (3 - 2 * fx);
            float sy = fy * fy * (3 - 2 * fy);
            float lowerBlend = llGrad + (lrGrad - llGrad) * sx;
            float upperBlend = ulGrad + (urGrad - ulGrad) * sx;
            return lowerBlend + (upperBlend - lowerBlend) * sy;
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe float EvalGradient(int hash, float fx, float fy)
        {
            int xHash = (hash & Const.GradAndMask) | Const.GradOrMask;
            int yHash = xHash << Const.GradShift1;
            return fx * *(float*)&xHash + fy * *(float*)&yHash; // dot-product
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static int Hash(int x, int y)
        {
            int hash = x ^ (y << 6);
            hash += hash >> 5;
            hash *= Const.XPrime1;
            hash ^= hash >> 4;
            hash += hash >> 2;
            hash ^= hash >> 16;
            hash *= Const.XPrime2;
            return hash;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static float Lerp(float a, float b, float t) => a + (b - a) * t;

        public static class Const
        {
            public const int FractalOctaves = 8;

            internal const int
                Offset = 0228125273,
                SeedPrime = 525124619,
                SeedMask = 0x0FFFFFFF,
                XPrime1 = 0863909317,
                YPrime1 = 1987438051,
                ZPrime1 = 1774326877,
                XPlusYPrime1 = unchecked(XPrime1 + YPrime1),
                XPlusZPrime1 = unchecked(XPrime1 + ZPrime1),
                YPlusZPrime1 = unchecked(YPrime1 + ZPrime1),
                XPlusYPlusZPrime1 = unchecked(XPrime1 + YPrime1 + ZPrime1),
                XMinusYPrime1 = unchecked(XPrime1 - YPrime1),
                YMinusXPrime1 = unchecked(XPrime1 - YPrime1),
                XPrime2 = 1299341299,
                YPrime2 = 0580423463,
                ZPrime2 = 0869819479,
                XPlusYPrime2 = unchecked(XPrime2 + YPrime2),
                XPlusZPrime2 = unchecked(XPrime2 + ZPrime2),
                YPlusZPrime2 = unchecked(YPrime2 + ZPrime2),
                XPlusYPlusZPrime2 = unchecked(XPrime2 + YPrime2 + ZPrime2),
                XMinusYPrime2 = unchecked(XPrime2 - YPrime2),
                YMinusXPrime2 = unchecked(XPrime2 - YPrime2),
                GradAndMask = -0x7F9FE7F9, //-0x7F87F801
                GradOrMask = 0x3F0FC3F0, //0x3F03F000
                GradShift1 = 10,
                GradShift2 = 20,
                PeriodShift = 18,
                WorleyAndMask = 0x007803FF,
                WorleyOrMask = 0x3F81FC00,
                PortionAndMask = 0x007FFFFF,
                PortionOrMask = 0x3F800000;
        }

        public readonly struct NoisePeriod
        {
            internal readonly int xf, yf, zf;
            public int XPeriod => xf == 0 ? 0 : (int)(uint.MaxValue / xf + 1);
            public int YPeriod => yf == 0 ? 0 : (int)(uint.MaxValue / yf + 1);
            public int ZPeriod => zf == 0 ? 0 : (int)(uint.MinValue / zf + 1);
            public NoisePeriod(int xPeriod, int yPeriod, int zPeriod = 0)
            {
                xf = GetFactor(xPeriod);
                yf = GetFactor(yPeriod);
                zf = GetFactor(zPeriod);
                static unsafe int GetFactor(int period)
                {
                    if (period == 0)
                        return 0;
                    if (period <= 1)
                        throw new System.ArgumentException($"period '{period}' must be greater then 1.");
                    uint factor = (uint.MaxValue / (uint)period);
                    factor += 1;
                    return *(int*)&factor;
                }
            }
            public bool IsNull => xf == 0;
            public static readonly NoisePeriod Null = default;
            internal const int ByteSize = 16;
        }

        [MethodImpl(512)] // aggressive optimization on supported runtimes
        public static float GradientNoiseHQ(float x, float y, int seed = 0)
        {
            // rotation from https://noiseposti.ng/posts/2022-01-16-The-Perlin-Problem-Breaking-The-Cycle.html
            float xy = x + y;
            float s2 = xy * -0.2113248f;
            float z = xy * -0.5773502f;
            x += s2;
            y += s2;

            int ix = x > 0 ? (int)x : (int)x - 1;
            int iy = y > 0 ? (int)y : (int)y - 1;
            int iz = z > 0 ? (int)z : (int)z - 1;
            float fx = x - ix;
            float fy = y - iy;
            float fz = z - iz;

            ix += seed * Const.SeedPrime;

            ix += Const.Offset;
            iy += Const.Offset;
            iz += Const.Offset;
            int p1 = ix * Const.XPrime1 + iy * Const.YPrime1 + iz * Const.ZPrime1;
            int p2 = ix * Const.XPrime2 + iy * Const.YPrime2 + iz * Const.ZPrime2;
            int llHash = p1 * p2;
            int lrHash = (p1 + Const.XPrime1) * (p2 + Const.XPrime2);
            int ulHash = (p1 + Const.YPrime1) * (p2 + Const.YPrime2);
            int urHash = (p1 + Const.XPlusYPrime1) * (p2 + Const.XPlusYPrime2);
            float zLowBlend = InterpolateGradients3D(llHash, lrHash, ulHash, urHash, fx, fy, fz);
            llHash = (p1 + Const.ZPrime1) * (p2 + Const.ZPrime2);
            lrHash = (p1 + Const.XPlusZPrime1) * (p2 + Const.XPlusZPrime2);
            ulHash = (p1 + Const.YPlusZPrime1) * (p2 + Const.YPlusZPrime2);
            urHash = (p1 + Const.XPlusYPlusZPrime1) * (p2 + Const.XPlusYPlusZPrime2);
            float zHighBlend = InterpolateGradients3D(llHash, lrHash, ulHash, urHash, fx, fy, fz - 1);
            float sz = fz * fz * (3 - 2 * fz);
            return zLowBlend + (zHighBlend - zLowBlend) * sz;
        }

        [MethodImpl(512)] // aggressive optimization on supported runtimes
        public static float GradientNoise3D(float x, float y, float z, int seed = 0)
        {
            // see comments in GradientNoise()
            int ix = x > 0 ? (int)x : (int)x - 1;
            int iy = y > 0 ? (int)y : (int)y - 1;
            int iz = z > 0 ? (int)z : (int)z - 1;
            float fx = x - ix;
            float fy = y - iy;
            float fz = z - iz;

            ix += seed * Const.SeedPrime;

            ix += Const.Offset;
            iy += Const.Offset;
            iz += Const.Offset;
            int p1 = ix * Const.XPrime1 + iy * Const.YPrime1 + iz * Const.ZPrime1;
            int p2 = ix * Const.XPrime2 + iy * Const.YPrime2 + iz * Const.ZPrime2;
            int llHash = p1 * p2;
            int lrHash = (p1 + Const.XPrime1) * (p2 + Const.XPrime2);
            int ulHash = (p1 + Const.YPrime1) * (p2 + Const.YPrime2);
            int urHash = (p1 + Const.XPlusYPrime1) * (p2 + Const.XPlusYPrime2);
            float zLowBlend = InterpolateGradients3D(llHash, lrHash, ulHash, urHash, fx, fy, fz);
            llHash = (p1 + Const.ZPrime1) * (p2 + Const.ZPrime2);
            lrHash = (p1 + Const.XPlusZPrime1) * (p2 + Const.XPlusZPrime2);
            ulHash = (p1 + Const.YPlusZPrime1) * (p2 + Const.YPlusZPrime2);
            urHash = (p1 + Const.XPlusYPlusZPrime1) * (p2 + Const.XPlusYPlusZPrime2);
            float zHighBlend = InterpolateGradients3D(llHash, lrHash, ulHash, urHash, fx, fy, fz - 1);
            float sz = fz * fz * (3 - 2 * fz);
            return zLowBlend + (zHighBlend - zLowBlend) * sz;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static unsafe float InterpolateGradients3D(int llHash, int lrHash, int ulHash, int urHash, float fx, float fy, float fz)
        {
            int xHash, yHash, zHash;
            xHash = (llHash & Const.GradAndMask) | Const.GradOrMask;
            yHash = xHash << Const.GradShift1;
            zHash = xHash << Const.GradShift2;
            float llGrad = fx * *(float*)&xHash + fy * *(float*)&yHash + fz * *(float*)&zHash; // dot-product
            xHash = (lrHash & Const.GradAndMask) | Const.GradOrMask;
            yHash = xHash << Const.GradShift1;
            zHash = xHash << Const.GradShift2;
            float lrGrad = (fx - 1) * *(float*)&xHash + fy * *(float*)&yHash + fz * *(float*)&zHash;
            xHash = (ulHash & Const.GradAndMask) | Const.GradOrMask;
            yHash = xHash << Const.GradShift1;
            zHash = xHash << Const.GradShift2;
            float ulGrad = fx * *(float*)&xHash + (fy - 1) * *(float*)&yHash + fz * *(float*)&zHash; // dot-product
            xHash = (urHash & Const.GradAndMask) | Const.GradOrMask;
            yHash = xHash << Const.GradShift1;
            zHash = xHash << Const.GradShift2;
            float urGrad = (fx - 1) * *(float*)&xHash + (fy - 1) * *(float*)&yHash + fz * *(float*)&zHash;
            float sx = fx * fx * (3 - 2 * fx);
            float sy = fy * fy * (3 - 2 * fy);
            float lowerBlend = llGrad + (lrGrad - llGrad) * sx;
            float upperBlend = ulGrad + (urGrad - ulGrad) * sx;
            return lowerBlend + (upperBlend - lowerBlend) * sy;
        }
    }
}
