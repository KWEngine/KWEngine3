/* Audio (for KWEngine2)
 *  
 * Written by: Lutz Karau <lutz.karau@gmail.com>
 * Licence: GNU LGPL 2.1
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KWEngine3.Audio
{
    class WaveFormat
    {
        /// <summary>
        /// Sample Rate
        /// </summary>
        public int SampleRate { get; set; } = -1;
        /// <summary>
        /// Bits per Sample
        /// </summary>
        public int BitsPerSample { get; set; } = -1;
        /// <summary>
        /// Mono or Stereo
        /// </summary>
        public int Channels { get; set; } = -1;

        /// <summary>
        /// Creates a WaveFormat for given Parameters
        /// </summary>
        /// <param name="samplerate">Sample rate (i.e. 44100)</param>
        /// <param name="bitspersample">Bits per sample (i.e. 16)</param>
        /// <param name="channels">Channel count (i.e. 1 or 2)</param>
        public WaveFormat(int samplerate, int bitspersample, int channels)
        {
            SampleRate = samplerate;
            BitsPerSample = bitspersample;
            Channels = channels;
        }
    }
}
