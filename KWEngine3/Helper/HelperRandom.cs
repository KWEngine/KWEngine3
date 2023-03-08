using System;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Helferklasse für Zufallszahlen
    /// </summary>
    public static class HelperRandom
    {
        /// <summary>
        /// Generatorfeldvariable
        /// </summary>
        private static Random generator = new Random(DateTime.Now.Millisecond);

        /// <summary>
        /// Berechnet eine Zufallszahl zwischen zwei Werten (beide inklusive)
        /// </summary>
        /// <param name="min">(inklusive) Untergrenze</param>
        /// <param name="max">(inklusive) Obergrenze</param>
        /// <returns>Zufallszahl</returns>
        public static float GetRandomNumber(float min, float max)
        {
            return (float)((generator.NextDouble() * (max - min)) + min);
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
