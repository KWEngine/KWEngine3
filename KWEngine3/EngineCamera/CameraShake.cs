using KWEngine3.Helper;
using OpenTK.Mathematics;

namespace KWEngine3.EngineCamera
{
    /// <summary>
    /// Camera shake as a small rotation of the view around the eye (yaw, pitch, roll in degrees).
    /// The eye position never moves, so everything derived from the camera position (lighting, fog,
    /// shadow cascades, foliage LOD) stays exact and the projection stays symmetric (SSAO reconstruction).
    /// Strength decays with the square of the remaining time ("trauma" model):
    ///   angle = amplitude * s^2 * noise(phase + t * frequency), s = 1 - elapsed / duration
    /// </summary>
    internal struct CameraShake
    {
        internal const float MAXANGLE = 30f;            // degrees per axis
        internal const float MINFREQUENCY = 1f;
        internal const float MAXFREQUENCY = 60f;
        private const float NOISESCALE = 1.2f;          // HelperPerlinNoise.GradientNoise peaks at about +/-0.85 -> peaks of about +/-1
        private const float CULLINGSAFETY = 1.02f;      // extra reserve on top of the worst case at the corners of the angle box
        private const float PHASEWRAP = 4096f;          // keeps the noise coordinate small enough for float precision
        private const float MAXCULLINGTAN = 11.43f;     // tan(85 degrees): beyond that the shaken view is not culled at all

        internal Vector3 _amplitude;                    // yaw, pitch, roll (degrees) at _timestamp
        internal float _timestamp;                      // world time at which _amplitude and _phase were valid
        internal float _duration;                       // seconds from _timestamp until the shake has decayed to zero
        internal float _frequency;                      // noise units per second
        internal float _phase;                          // noise coordinate at _timestamp

        /// <summary>
        /// Remaining strength factor (1 at _timestamp, 0 at the end of the shake)
        /// </summary>
        internal readonly float GetDecay(float worldTime)
        {
            float elapsed = worldTime - _timestamp;
            if (_duration <= 0f || elapsed < 0f || elapsed >= _duration)
                return 0f;
            float s = 1f - elapsed / _duration;
            return s * s;
        }

        /// <summary>
        /// Upper bound of the absolute shake angles (degrees) at the given time
        /// </summary>
        internal readonly Vector3 GetEnvelope(float worldTime)
        {
            return _amplitude * GetDecay(worldTime);
        }

        /// <summary>
        /// Current shake angles (degrees): x = yaw, y = pitch, z = roll
        /// </summary>
        internal readonly Vector3 GetAngles(float worldTime)
        {
            float decay = GetDecay(worldTime);
            if (decay <= 0f)
                return Vector3.Zero;

            float x = _phase + (worldTime - _timestamp) * _frequency;
            return new Vector3(
                _amplitude.X * decay * Noise(x, 0),
                _amplitude.Y * decay * Noise(x, 1),
                _amplitude.Z * decay * Noise(x, 2)
                );
        }

        /// <summary>
        /// Starts a new shake or merges it into the running one
        /// </summary>
        /// <param name="amplitude">maximum angles in degrees (yaw, pitch, roll), already clamped by the caller</param>
        /// <param name="duration">seconds</param>
        /// <param name="frequency">noise units per second, already clamped by the caller</param>
        /// <param name="additive">true: add to the remaining strength of a running shake; false: replace it</param>
        /// <param name="worldTime">current world time</param>
        internal void Start(Vector3 amplitude, float duration, float frequency, bool additive, float worldTime)
        {
            float decay = GetDecay(worldTime);

            float phase;
            if (decay > 0f)
            {
                // continue on the running noise curve so that the camera does not jump
                phase = _phase + (worldTime - _timestamp) * _frequency;
                if (phase > PHASEWRAP)
                    phase -= PHASEWRAP;
            }
            else
            {
                // start somewhere else on the noise curve so that consecutive shakes do not look identical
                phase = Random.Shared.NextSingle() * PHASEWRAP;
            }

            if (additive && decay > 0f)
            {
                Vector3 remainingAmplitude = _amplitude * decay;
                float remainingDuration = _duration - (worldTime - _timestamp);
                _amplitude = Vector3.ComponentMin(remainingAmplitude + amplitude, new Vector3(MAXANGLE));
                _duration = Math.Max(remainingDuration, duration);
            }
            else
            {
                _amplitude = amplitude;
                _duration = duration;
            }
            _timestamp = worldTime;
            _frequency = frequency;
            _phase = phase;
        }

        /// <summary>
        /// Ends a running shake immediately
        /// </summary>
        internal void Stop()
        {
            _amplitude = Vector3.Zero;
            _duration = 0f;
        }

        private static float Noise(float x, int axis)
        {
            // 1D slice through the 2D gradient noise; y stays off the integer lattice lines where the noise is always zero
            float n = HelperPerlinNoise.GradientNoise(x, 0.37f + axis * 3.11f, axis) * NOISESCALE;
            return Math.Clamp(n, -1f, 1f);
        }

        /// <summary>
        /// Rotation in view space around the eye (view space origin): yaw around the view y axis, pitch around the view x axis, roll around the view z axis.
        /// Usage: shakenView = view * CreateRotation(angles)
        /// </summary>
        internal static Matrix4 CreateRotation(Vector3 anglesDegrees)
        {
            return Matrix4.CreateRotationY(MathHelper.DegreesToRadians(anglesDegrees.X))
                * Matrix4.CreateRotationX(MathHelper.DegreesToRadians(anglesDegrees.Y))
                * Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(anglesDegrees.Z));
        }

        /// <summary>
        /// Builds a symmetric perspective projection for the unshaken view matrix whose frustum contains
        /// the frustum of every shaken view with |angles| &lt;= envelope (used for render pass culling only).
        /// Returns false if field of view plus shake angle get too close to 90 degrees; culling should be skipped then.
        /// </summary>
        internal static bool TryCreateCullingProjection(Matrix4 projection, Vector3 envelopeDegrees, float zNear, float zFar, out Matrix4 cullingProjection)
        {
            float tanX = 1f / projection.M11;
            float tanY = 1f / projection.M22;
            float maxTanX = tanX;
            float maxTanY = tanY;
            float minDepth = 1f;
            float maxDepth = 1f;
            cullingProjection = Matrix4.Identity;

            // The shaken view sees q = p * R (p = unshaken view space, R = yaw * pitch * roll). The visible
            // directions in unshaken view space are the frustum corner rays multiplied by R^-1 (roll is undone
            // first, yaw last). Yaw and pitch reach their extremes at the ends of their ranges, roll can also
            // peak inside its range (where tanX * cos + tanY * sin is maximal), so those angles are sampled too.
            Span<float> yawPitchSamples = stackalloc float[] { -1f, 0f, 1f };
            Span<float> rollSamples = stackalloc float[9];
            float rollMax = MathHelper.DegreesToRadians(envelopeDegrees.Z);
            float rollCritical1 = MathF.Atan(tanY / tanX);
            float rollCritical2 = MathF.Atan(tanX / tanY);
            rollSamples[0] = -rollMax;
            rollSamples[1] = -rollMax * 0.5f;
            rollSamples[2] = 0f;
            rollSamples[3] = rollMax * 0.5f;
            rollSamples[4] = rollMax;
            rollSamples[5] = -Math.Min(rollCritical1, rollMax);
            rollSamples[6] = Math.Min(rollCritical1, rollMax);
            rollSamples[7] = -Math.Min(rollCritical2, rollMax);
            rollSamples[8] = Math.Min(rollCritical2, rollMax);

            for (int iy = 0; iy < yawPitchSamples.Length; iy++)
            {
                for (int ip = 0; ip < yawPitchSamples.Length; ip++)
                {
                    for (int ir = 0; ir < rollSamples.Length; ir++)
                    {
                        Vector3 angles = new(
                            yawPitchSamples[iy] * envelopeDegrees.X,
                            yawPitchSamples[ip] * envelopeDegrees.Y,
                            MathHelper.RadiansToDegrees(rollSamples[ir])
                            );
                        Matrix4 inverseRotation = Matrix4.Transpose(CreateRotation(angles));

                        for (int c = 0; c < 4; c++)
                        {
                            Vector4 corner = new((c & 1) == 0 ? -tanX : tanX, (c & 2) == 0 ? -tanY : tanY, -1f, 0f);
                            Vector4 d = Vector4.TransformRow(corner, inverseRotation);
                            float depth = -d.Z;
                            if (depth < 0.05f)
                                return false;
                            maxTanX = Math.Max(maxTanX, Math.Abs(d.X) / depth);
                            maxTanY = Math.Max(maxTanY, Math.Abs(d.Y) / depth);
                            minDepth = Math.Min(minDepth, depth);
                            maxDepth = Math.Max(maxDepth, depth);
                        }
                    }
                }
            }

            maxTanX *= CULLINGSAFETY;
            maxTanY *= CULLINGSAFETY;
            if (maxTanX > MAXCULLINGTAN || maxTanY > MAXCULLINGTAN)
                return false;

            float near = zNear * minDepth / CULLINGSAFETY;
            float far = zFar * maxDepth * CULLINGSAFETY;
            cullingProjection = Matrix4.CreatePerspectiveOffCenter(-maxTanX * near, maxTanX * near, -maxTanY * near, maxTanY * near, near, far);
            return true;
        }
    }
}
