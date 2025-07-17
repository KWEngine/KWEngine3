using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        internal void Set(float start, float end, float db)
        {
            FrequencyStart = start;
            FrequencyEnd = end;
            Decibel = db;
        }
    }
}
