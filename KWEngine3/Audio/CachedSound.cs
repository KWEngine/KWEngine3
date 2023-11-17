/* AudioEngine (for KWEngine)
 *  
 * Written by: Lutz Karau <lutz.karau@gmail.com>
 * Licence: GNU LGPL 2.1
 */

using OpenTK.Audio.OpenAL;
using System;
using System.Runtime.InteropServices;
using System.IO;
using KWEngine3.Audio.OggDecoder;

namespace KWEngine3.Audio
{
    /// <summary>
    /// Vorgeladener Audioton
    /// </summary>
    internal class CachedSound : IDisposable
    {
        private readonly ALFormat mFormat;
        private readonly int mPointerBuffer;
        private readonly string mName;
        private readonly IntPtr pointerToAudioData;

        /// <summary>
        /// Audiodaten
        /// </summary>
        public byte[] AudioData { get; private set; }
        /// <summary>
        /// Waveformat
        /// </summary>
        public WaveFormat WaveFormat { get; private set; }

        /// <summary>
        /// Konstruktor für das Erzeugen eines gecacheten Tons
        /// </summary>
        /// <param name="audioFileName">Dateiname</param>
        public CachedSound(string audioFileName) 
        {
            int numChannels;
            int bitsPerSample;
            int sampleRate;
            if (!(audioFileName.ToLower().EndsWith("wav") || audioFileName.ToLower().EndsWith("ogg")))
                throw new Exception("Only wav and ogg files are supported.");

            if (audioFileName.ToLower().EndsWith("ogg"))
            {
                AudioData = ReadOggFile(audioFileName, out numChannels, out sampleRate, out bitsPerSample);
            }
            else
            {
                AudioData = ReadWaveFile(audioFileName, out numChannels, out sampleRate, out bitsPerSample);
            }
            
            WaveFormat = new WaveFormat(sampleRate, bitsPerSample, numChannels);
            if(bitsPerSample == 8)
            {
                if (numChannels == 1)
                {
                    mFormat = ALFormat.Mono8;
                }
                else if (numChannels == 2)
                {
                    mFormat = ALFormat.Stereo8;
                }
                else
                {
                    throw new Exception("Only mono and stereo wave (*.wav) sources are supported.");
                }
            }
            else if(bitsPerSample == 16)
            {
                if (numChannels == 1)
                {
                    mFormat = ALFormat.Mono16;
                }
                else if (numChannels == 2)
                {
                    mFormat = ALFormat.Stereo16;
                }
                else
                {
                    throw new Exception("Only mono and stereo wave (*.wav) sources are supported.");
                }
            }
            else
            {
                throw new Exception("Only 8bit mono and stereo wave (*.wav) sources are supported.");
            }
            
            mName = audioFileName.Substring(audioFileName.LastIndexOf(Path.DirectorySeparatorChar) + 1);

            mPointerBuffer = AL.GenBuffer();
            pointerToAudioData = Marshal.AllocHGlobal(AudioData.Length);
            Marshal.Copy(AudioData, 0, pointerToAudioData, AudioData.Length);
            AL.BufferData(mPointerBuffer, mFormat, pointerToAudioData, AudioData.Length, WaveFormat.SampleRate);
        }

        private static byte[] ReadOggFile(string filename, out int numChannels, out int sampleRate, out int bitsPerSample)
        {
            FileStream input = new FileStream(filename, FileMode.Open, FileAccess.Read);
            OggDecoder.OggDecodeStream stream = new OggDecoder.OggDecodeStream(input, true);
            byte[] inputAudioData = new byte[stream.Length];
            int read = stream.Read(inputAudioData, 0, inputAudioData.Length);
            if(read > 0)
            {
                numChannels = stream.ChannelCount;
                sampleRate = stream.SampleRate;
                bitsPerSample = stream.BitsPerSample;
                return inputAudioData;
            }
            throw new Exception("Cannot read OGG file. Must be 16 bits 44100hz stereo.");
        }

        private static byte[] ReadWaveFile(string filename, out int numChannels, out int sampleRate, out int bitsPerSample)
        {
            FileStream fStream = null;
            byte[] inputAudioData = null;
            try
            {
                fStream = new FileStream(filename, FileMode.Open) {Position = 22};

                // Read channels
                byte channels1 = (byte)fStream.ReadByte();
                fStream.Position = 23;
                byte channels2 = (byte)fStream.ReadByte();
                numChannels = (byte)(channels2 << 8 | channels1);

                // Read sample rate
                fStream.Position = 24;
                byte sRate1 = (byte)fStream.ReadByte();
                fStream.Position = 25;
                byte sRate2 = (byte)fStream.ReadByte();
                fStream.Position = 26;
                byte sRate3 = (byte)fStream.ReadByte();
                fStream.Position = 27;
                byte sRate4 = (byte)fStream.ReadByte();
                sampleRate = (int)(sRate4 << 24 | sRate3 << 16 | sRate2 << 8 | sRate1);


                // Read bits per sample
                fStream.Position = 34;
                byte bits1 = (byte)fStream.ReadByte();
                fStream.Position = 35;
                byte bits2 = (byte)fStream.ReadByte();
                bitsPerSample = (byte)(bits2 << 8 | bits1);

                int startpos = 36;
                while (startpos < 2048)
                {
                    fStream.Position = startpos;
                    byte[] audiochunk = new byte[4];
                    fStream.Read(audiochunk, 0, 4);
                    if (audiochunk[0] == 0x64 && audiochunk[1] == 0x61 && audiochunk[2] == 0x74 && audiochunk[3] == 0x61)
                    {
                        startpos += 8;
                        break;
                    }
                    else
                    {
                        startpos++;
                    }
                }

                fStream.Position = startpos;
                long bytesToRead = fStream.Length - startpos;
                inputAudioData = new byte[bytesToRead];
                fStream.Read(inputAudioData, 0, (int)bytesToRead);
            }
            catch (Exception ex)
            {
                inputAudioData = null;
                numChannels = 0;
                sampleRate = 0;
                bitsPerSample = 0;
                KWEngine.LogWriteLine("Error: Could not open file. (" + ex.Message + ")");
            }
            finally
            {
                if (fStream != null)
                {
                    try
                    {
                        fStream.Close();
                    }
                    catch (Exception)
                    {

                    }
                }
            }
            return inputAudioData;
        }

        public string GetName()
        {
            return mName;
        }

        public int GetChannels()
        {
            return WaveFormat.Channels;
        }

        public int GetSampleRate()
        {
            return WaveFormat.SampleRate;
        }

        public ALFormat GetFormat()
        {
            return mFormat;
        }

        public int GetBufferPointer()
        {
            return mPointerBuffer;
        }
        
        public void Clear()
        {
            AL.DeleteBuffer(mPointerBuffer);
            Marshal.FreeHGlobal(pointerToAudioData);
        }

        public void Dispose()
        {
            Clear();
        }
    }
}
