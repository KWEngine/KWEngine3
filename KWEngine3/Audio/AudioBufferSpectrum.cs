namespace KWEngine3.Audio
{
    internal class AudioBufferSpectrum
    {
        public bool IsValid { get; set; }
        public float TimestampApplication { get; internal set; }
        public float TimestampWorld { get; internal set; }
        public float Volume { get; internal set; }
        internal AudioBufferSpectrumBand[] Bands { get; set; }

        public AudioBufferSpectrum()
            : this(20)
        {

        }

        public AudioBufferSpectrum(int numBands)
        {
            TimestampApplication = 0;
            TimestampWorld = 0;
            IsValid = false;
            Volume = 0;
            Bands = new AudioBufferSpectrumBand[numBands];
            for (int i = 0; i < 20; i++)
            {
                Bands[i] = new AudioBufferSpectrumBand();
            }
        }

        internal void ConfigureBand(int bandIndex, float frequencyStart, float frequencyEnd, float db)
        {
            Bands[bandIndex].FrequencyStart = frequencyStart;
            Bands[bandIndex].FrequencyEnd = frequencyEnd;
            Bands[bandIndex].Decibel = db;
        }
    }
}
