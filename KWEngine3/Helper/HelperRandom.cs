using System;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Helferklasse für Zufallszahlen
    /// </summary>
    public static class HelperRandom
    {
        internal static Random generator = new Random(DateTime.Now.Millisecond);
        internal static float pnoise = -1.0f;

        /// <summary>
        /// Generiert eine Zufallszahl nach Ken Perlins Noise Generator
        /// </summary>
        /// <param name="speed">Steigung der Zufallszahlenänderung</param>
        /// <param name="min">Untergrenze</param>
        /// <param name="max">Obergrenze</param>
        /// <returns>Zufallszahl</returns>
        public static float GetRandomNumberFromPerlinNoise(float speed = 0.1f, float min = 0f, float max = 1f)
        {
            speed = Math.Clamp(speed, 0f, 1f);
            float rand = ((HelperPerlinNoise.GradientNoise(pnoise, pnoise, 3) * 2f) + 1f) * 0.5f * (max - min) + min;
            pnoise = pnoise + speed;
            if(pnoise > 1f)
            {
                float delta = pnoise - 1f;
                pnoise = -1f + delta;
            }
            return rand;
        }

        /// <summary>
        /// Berechnet eine Zufallszahl zwischen zwei Werten (beide inklusive)
        /// </summary>
        /// <param name="min">(inklusive) Untergrenze</param>
        /// <param name="max">(inklusive) Obergrenze</param>
        /// <returns>Zufallszahl</returns>
        public static float GetRandomNumber(float min, float max)
        {
            return generator.NextSingle() * (max - min) + min;
        }

        /// <summary>
        /// Berechnet eine Zufallszahl zwischen zwei Ganzzahlwerten (beide inklusive)
        /// </summary>
        /// <param name="min">(inklusive) Untergrenze</param>
        /// <param name="max">(inklusive) Obergrenze</param>
        /// <returns></returns>
        public static int GetRandomNumber(int min, int max)
        {
            return generator.Next(min, max + 1);
        }
    }
}
