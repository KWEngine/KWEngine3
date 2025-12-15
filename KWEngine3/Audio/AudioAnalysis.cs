using KWEngine3.Exceptions;

namespace KWEngine3.Audio
{
    /// <summary>
    /// Analysedatencontainer für die spektrografische Audioanalyse
    /// </summary>
    public struct AudioAnalysis
    {
        /// <summary>
        /// Gibt an, ob der Datensatz gültig ist
        /// </summary>
        public bool IsValid { get; set; }
        /// <summary>
        /// Messzeitpunkt in Relation zum Anwendungsstartzeitpunkt
        /// </summary>
        public float TimestampApplication { get; internal set; }
        /// <summary>
        /// Messzeitpunkt in Relation zum Weltstartzeitpunkt
        /// </summary>
        public float TimestampWorld { get; internal set; }
        /// <summary>
        /// Lautstärkewert (0.0f = lautlos, 1.0f = laut)
        /// </summary>
        public float Volume { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 1
        /// </summary>
        public AudioAnalysisBand Band01 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 2
        /// </summary>
        public AudioAnalysisBand Band02 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 3
        /// </summary>
        public AudioAnalysisBand Band03 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 4
        /// </summary>
        public AudioAnalysisBand Band04 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 5
        /// </summary>
        public AudioAnalysisBand Band05 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 6
        /// </summary>
        public AudioAnalysisBand Band06 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 7
        /// </summary>
        public AudioAnalysisBand Band07 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 8
        /// </summary>
        public AudioAnalysisBand Band08 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 9
        /// </summary>
        public AudioAnalysisBand Band09 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 10
        /// </summary>
        public AudioAnalysisBand Band10 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 11
        /// </summary>
        public AudioAnalysisBand Band11 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 12
        /// </summary>
        public AudioAnalysisBand Band12 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 13
        /// </summary>
        public AudioAnalysisBand Band13 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 14
        /// </summary>
        public AudioAnalysisBand Band14 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 15
        /// </summary>
        public AudioAnalysisBand Band15 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 16
        /// </summary>
        public AudioAnalysisBand Band16 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 17
        /// </summary>
        public AudioAnalysisBand Band17 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 18
        /// </summary>
        public AudioAnalysisBand Band18 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 19
        /// </summary>
        public AudioAnalysisBand Band19 { get; internal set; }
        /// <summary>
        /// Daten zu Frequenzband 20
        /// </summary>
        public AudioAnalysisBand Band20 { get; internal set; }

        /// <summary>
        /// Erfragt die Daten zu einem nullbasierten Frequenzbandindex (0 = Band #1, 1 = Band #2, usw.)
        /// </summary>
        /// <param name="band">Nullbasierter Band-Index</param>
        /// <returns>Daten zum erfragten Frequenzband</returns>
        public AudioAnalysisBand GetBand(int band)
        {
            band = Math.Clamp(band, 0, 19);
            return band switch
            {
                0 => Band01,
                1 => Band02,
                2 => Band03,
                3 => Band04,
                4 => Band05,
                5 => Band06,
                6 => Band07,
                7 => Band08,
                8 => Band09,
                9 => Band10,
                10 => Band11,
                11 => Band12,
                12 => Band13,
                13 => Band14,
                14 => Band15,
                15 => Band16,
                16 => Band17,
                17 => Band18,
                18 => Band19,
                19 => Band20,
                _ => throw new EngineException("[Audio] Invalid band index")
            };
        }

        /// <summary>
        /// Konstruktormethode
        /// </summary>
        public AudioAnalysis()
        {
            TimestampApplication = 0;
            TimestampWorld = 0;
            IsValid = false;
            Volume = 0;

            Band01 = new AudioAnalysisBand();
            Band02 = new AudioAnalysisBand();
            Band03 = new AudioAnalysisBand();
            Band04 = new AudioAnalysisBand();
            Band05 = new AudioAnalysisBand();
            Band06 = new AudioAnalysisBand();
            Band07 = new AudioAnalysisBand();
            Band08 = new AudioAnalysisBand();
            Band09 = new AudioAnalysisBand();
            Band10 = new AudioAnalysisBand();
            Band11 = new AudioAnalysisBand();
            Band12 = new AudioAnalysisBand();
            Band13 = new AudioAnalysisBand();
            Band14 = new AudioAnalysisBand();
            Band15 = new AudioAnalysisBand();
            Band16 = new AudioAnalysisBand();
            Band17 = new AudioAnalysisBand();
            Band18 = new AudioAnalysisBand();
            Band19 = new AudioAnalysisBand();
            Band20 = new AudioAnalysisBand();
        }

        internal void Fill(AudioBufferSpectrum src)
        {
            IsValid = src.IsValid;
            if (IsValid)
            {
                Band01 = new AudioAnalysisBand() { FrequencyStart = src.Bands[0].FrequencyStart, FrequencyEnd = src.Bands[0].FrequencyEnd, Decibel = src.Bands[0].Decibel, Index = 0 };
                Band02 = new AudioAnalysisBand() { FrequencyStart = src.Bands[1].FrequencyStart, FrequencyEnd = src.Bands[1].FrequencyEnd, Decibel = src.Bands[1].Decibel, Index = 1 };
                Band03 = new AudioAnalysisBand() { FrequencyStart = src.Bands[2].FrequencyStart, FrequencyEnd = src.Bands[2].FrequencyEnd, Decibel = src.Bands[2].Decibel, Index = 2 };
                Band04 = new AudioAnalysisBand() { FrequencyStart = src.Bands[3].FrequencyStart, FrequencyEnd = src.Bands[3].FrequencyEnd, Decibel = src.Bands[3].Decibel, Index = 3 };
                Band05 = new AudioAnalysisBand() { FrequencyStart = src.Bands[4].FrequencyStart, FrequencyEnd = src.Bands[4].FrequencyEnd, Decibel = src.Bands[4].Decibel, Index = 4 };
                Band06 = new AudioAnalysisBand() { FrequencyStart = src.Bands[5].FrequencyStart, FrequencyEnd = src.Bands[5].FrequencyEnd, Decibel = src.Bands[5].Decibel, Index = 5 };
                Band07 = new AudioAnalysisBand() { FrequencyStart = src.Bands[6].FrequencyStart, FrequencyEnd = src.Bands[6].FrequencyEnd, Decibel = src.Bands[6].Decibel, Index = 6 };
                Band08 = new AudioAnalysisBand() { FrequencyStart = src.Bands[7].FrequencyStart, FrequencyEnd = src.Bands[7].FrequencyEnd, Decibel = src.Bands[7].Decibel, Index = 7 };
                Band09 = new AudioAnalysisBand() { FrequencyStart = src.Bands[8].FrequencyStart, FrequencyEnd = src.Bands[8].FrequencyEnd, Decibel = src.Bands[8].Decibel, Index = 8 };
                Band10 = new AudioAnalysisBand() { FrequencyStart = src.Bands[9].FrequencyStart, FrequencyEnd = src.Bands[9].FrequencyEnd, Decibel = src.Bands[9].Decibel, Index = 9 };

                Band11 = new AudioAnalysisBand() { FrequencyStart = src.Bands[10].FrequencyStart, FrequencyEnd = src.Bands[10].FrequencyEnd, Decibel = src.Bands[10].Decibel, Index = 10 };
                Band12 = new AudioAnalysisBand() { FrequencyStart = src.Bands[11].FrequencyStart, FrequencyEnd = src.Bands[11].FrequencyEnd, Decibel = src.Bands[11].Decibel, Index = 11 };
                Band13 = new AudioAnalysisBand() { FrequencyStart = src.Bands[12].FrequencyStart, FrequencyEnd = src.Bands[12].FrequencyEnd, Decibel = src.Bands[12].Decibel, Index = 12 };
                Band14 = new AudioAnalysisBand() { FrequencyStart = src.Bands[13].FrequencyStart, FrequencyEnd = src.Bands[13].FrequencyEnd, Decibel = src.Bands[13].Decibel, Index = 13 };
                Band15 = new AudioAnalysisBand() { FrequencyStart = src.Bands[14].FrequencyStart, FrequencyEnd = src.Bands[14].FrequencyEnd, Decibel = src.Bands[14].Decibel, Index = 14 };
                Band16 = new AudioAnalysisBand() { FrequencyStart = src.Bands[15].FrequencyStart, FrequencyEnd = src.Bands[15].FrequencyEnd, Decibel = src.Bands[15].Decibel, Index = 15 };
                Band17 = new AudioAnalysisBand() { FrequencyStart = src.Bands[16].FrequencyStart, FrequencyEnd = src.Bands[16].FrequencyEnd, Decibel = src.Bands[16].Decibel, Index = 16 };
                Band18 = new AudioAnalysisBand() { FrequencyStart = src.Bands[17].FrequencyStart, FrequencyEnd = src.Bands[17].FrequencyEnd, Decibel = src.Bands[17].Decibel, Index = 17 };
                Band19 = new AudioAnalysisBand() { FrequencyStart = src.Bands[18].FrequencyStart, FrequencyEnd = src.Bands[18].FrequencyEnd, Decibel = src.Bands[18].Decibel, Index = 18 };
                Band20 = new AudioAnalysisBand() { FrequencyStart = src.Bands[19].FrequencyStart, FrequencyEnd = src.Bands[19].FrequencyEnd, Decibel = src.Bands[19].Decibel, Index = 19 };

                TimestampApplication = src.TimestampApplication;
                TimestampWorld = src.TimestampWorld;
                Volume = src.Volume;
            }
        }
    }
}
