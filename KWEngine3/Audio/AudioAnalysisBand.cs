namespace KWEngine3.Audio
{
    /// <summary>
    /// Daten eines Frequenzbands bei der spektrografischen Analyse
    /// </summary>
    public struct AudioAnalysisBand
    {
        /// <summary>
        /// Startfrequenz des Bands
        /// </summary>
        public float FrequencyStart;
        /// <summary>
        /// Endfrequenz des Bands
        /// </summary>
        public float FrequencyEnd;
        /// <summary>
        /// Gemessene Decibel
        /// </summary>
        public float Decibel;
        /// <summary>
        /// Index des Bands
        /// </summary>
        public int Index;

    }
}
