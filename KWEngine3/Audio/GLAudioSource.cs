/* Audio (for KWEngine3)
 *  
 * Written by: Lutz Karau <lutz.karau@gmail.com>
 * Licence: GNU LGPL 2.1
 */

using KWEngine3.Audio.Lomont;
using KWEngine3.Helper;
using OpenTK.Audio.OpenAL;

namespace KWEngine3.Audio
{
    internal class GLAudioSource
    {
        internal enum PlaybackState
        {
            Stopped = 0,
            Playing = 1,
            Paused = 2
        }

        private int BUFFER_COUNT = Math.Clamp(KWEngine.AudioBuffersPerChannel, 2, 4);

        //private byte[] _empty;
        private volatile int mSource = -1;
        private PlaybackState State
        {
            get
            {
                return _playState;
            }
            set
            {
                //Console.WriteLine("setting state to: " + value);
                _playState = value;
            }
        }
        private volatile PlaybackState _playState = PlaybackState.Stopped;
        private CachedSound mSound;
        private volatile Queue<int> mBuffers;
        private int mBytesPerBuffer;
        private int mReadPosition;
        private int mBytesTotal;
        private byte[] mTempBuffer;
        private double[] mAnalyzeBuffer;
        internal volatile LomontFFT _fft;
        internal AudioBufferSpectrum _currentSpectrum;
        private Thread _playbackThread;

        private double[] _magnitudesPerBin = new double[20];
        private int[] _magnitudesAddedPerBin = new int[20];
        private static float[] _binFrequencyMaximums =
            new float[]
            {
                50,
                90,
                130,
                176,
                240,
                330,
                450,
                560,
                650,
                850,
                1200,
                1600,
                2200,
                3000,
                4100,
                5600,
                7700,
                11000,
                14000,
                22050
            };

        public GLAudioSource()
        {
            mSource = AL.GenSource();
            mBuffers = new Queue<int>(BUFFER_COUNT);
            InitializeALBuffers();
            _fft = new LomontFFT() { A = 0, B = 1 };
            _currentSpectrum = new AudioBufferSpectrum(20);
        }



        private void InitializeALBuffers()
        {
            foreach (int buffer in mBuffers)
            {
                AL.DeleteBuffer(buffer);
            }
            mBuffers.Clear();

            for (int i = 0; i < BUFFER_COUNT; i++)
            {
                int bufferId = AL.GenBuffer();
                mBuffers.Enqueue(bufferId);
            }
        }

        public bool IsLooping { get; set; } = false;

        public void SetVolume(float volume)
        {
            AL.Source(mSource, ALSourcef.Gain, volume);
        }

        private void EraseBuffers()
        {
            AL.GetSource(mSource, ALGetSourcei.BuffersQueued, out int queued);
            if (queued > 0)
            {
                int[] buffers = new int[queued];
                AL.SourceUnqueueBuffers(mSource, queued, buffers);
            }
            
            /*unsafe
            {
                fixed (byte* ptr = _empty)
                { 
                    foreach (int buffer in mBuffers)
                    {
                        AL.BufferData(buffer, mSound.GetFormat(), ptr, _empty.Length, mSound.WaveFormat.SampleRate);
                    }
                }
            }*/
            
        }

        public void SetCachedSound(CachedSound sound)
        {
            if(mSound != null)
            {
                if(sound.GetName() != mSound.GetName())
                {
                    mSound = sound;
                    mBytesTotal = sound.AudioData.Length;
                    mBytesPerBuffer = CalculateBytesPerBuffer();
                    mTempBuffer = new byte[mBytesPerBuffer];
                    //byte value = 0;
                    //if (mSound.WaveFormat.BitsPerSample == 8) value = 128;
                    //_empty = new byte[mTempBuffer.Length];
                    //Array.Fill(_empty, value);

                    EraseBuffers();

                    int bytesInPublicBuffer = mBytesPerBuffer / mSound.WaveFormat.Channels / (mSound.WaveFormat.BitsPerSample == 16 ? 2 : 1);
                    mAnalyzeBuffer = new double[HelperTexture.RoundDownToPowerOf2(bytesInPublicBuffer)];
                }
            }
            else
            {
                mSound = sound;
                mBytesTotal = sound.AudioData.Length;
                mBytesPerBuffer = CalculateBytesPerBuffer();
                mTempBuffer = new byte[mBytesPerBuffer];
                //byte value = 0;
                //if (mSound.WaveFormat.BitsPerSample == 8) value = 128;
                //_empty = new byte[mTempBuffer.Length];
                //Array.Fill(_empty, value);

                EraseBuffers();

                int bytesInPublicBuffer = mBytesPerBuffer / mSound.WaveFormat.Channels / (mSound.WaveFormat.BitsPerSample == 16 ? 2 : 1);
                mAnalyzeBuffer = new double[HelperTexture.RoundDownToPowerOf2(bytesInPublicBuffer)];
            }
            mReadPosition = 0;
        }

        private int CalculateBytesPerBuffer()
        {
            int bytesPerBuffer = (int)MathF.Round(mSound.WaveFormat.SampleRate * (Audio.BufferSizeMs / 1000f) * mSound.WaveFormat.Channels * (mSound.WaveFormat.BitsPerSample == 8 ? 1f : 2f));
            return bytesPerBuffer;
        }

        private bool StreamIntoBuffer(int buffer)
        {
            int bytesReadTotal = 0;
            int bytesRead = SafeCopyBytes(mSound.AudioData, mReadPosition, mTempBuffer, 0, mBytesPerBuffer);
            bytesReadTotal += bytesRead;
            if (bytesRead < mBytesPerBuffer)
            {
                if (IsLooping)
                {
                    int delta = mBytesPerBuffer - bytesRead;
                    bytesRead = SafeCopyBytes(mSound.AudioData, 0, mTempBuffer, bytesRead, delta);
                    mReadPosition = delta;
                    bytesReadTotal += delta;
                }
                else
                {
                    mReadPosition += bytesRead;
                    byte value = mSound.WaveFormat.BitsPerSample == 16 ? (byte)0 : (byte)128;
                    Span<byte> span = mTempBuffer;
                    span.Slice(bytesRead).Fill(value);
                    bytesReadTotal += mBytesPerBuffer - bytesRead;
                }
            }
            else
            {
                mReadPosition += bytesRead;
            }
                

            lock (mTempBuffer)
            {
                unsafe
                {
                    fixed (byte* ptr = mTempBuffer)
                    {
                        AL.BufferData(buffer, mSound.GetFormat(), ptr, bytesReadTotal, mSound.WaveFormat.SampleRate);
                    }
                }
                AnalyseData();
            }

            if (!IsLooping)
            {
                if (mReadPosition == mBytesTotal)
                {
                    return false;
                }
                else
                    return true;
            }
            else
            {
                return true;
            }
        }

        private double Window(int i, double sample, int length)
        {
            sample *= WindowBlackman(i, length);
            return sample;
        }

        private double WindowBlackman(int i, int length)
        {
            double a0 = 0.42;
            double a1 = 0.5;
            double a2 = 0.08;

            double phase = 2 * Math.PI * i / (length - 1);

            return a0
                 - a1 * Math.Cos(phase)
                 + a2 * Math.Cos(2 * phase);
        }

        private double WindowNattall(int i, int length)
        {
            double a0 = 0.355768;
            double a1 = 0.487396;
            double a2 = 0.144232;
            double a3 = 0.012604;

            double factor = 2 * Math.PI * i / (length - 1);

            return a0
                - a1 * Math.Cos(factor)
                + a2 * Math.Cos(2 * factor)
                - a3 * Math.Cos(3 * factor);
        }

        private void AnalyseData()
        {
            if (Audio.AnalyserActive == false)
                return;

            double sumVolume = 0;
            if (mSound.WaveFormat.BitsPerSample == 16 && mSound.WaveFormat.Channels == 2)
            {
                for (int i = 0, j = 0; i < mAnalyzeBuffer.Length; i += 4, j++)
                {
                    short dataLeft = (short)((mTempBuffer[i + 1] << 8) | mTempBuffer[i + 0]);
                    short dataRight = (short)((mTempBuffer[i + 3] << 8) | mTempBuffer[i + 2]);
                    double dataMono = (dataLeft + dataRight) / 2.0 / short.MaxValue;
                    sumVolume += dataMono * dataMono;
                    mAnalyzeBuffer[j] = Window(j, dataMono, mAnalyzeBuffer.Length);
                }
            }
            else if (mSound.WaveFormat.BitsPerSample == 16 && mSound.WaveFormat.Channels == 1)
            {
                for (int i = 0, j = 0; i < mAnalyzeBuffer.Length; i += 2, j++)
                {
                    double dataMono = (short)((mTempBuffer[i + 1] << 8) | mTempBuffer[i + 0]) * 1.0 / short.MaxValue;
                    sumVolume += dataMono * dataMono;
                    mAnalyzeBuffer[j] = Window(j, dataMono, mAnalyzeBuffer.Length);
                }
            }
            else if (mSound.WaveFormat.BitsPerSample == 8 && mSound.WaveFormat.Channels == 2)
            {
                for (int i = 0, j = 0; i < mAnalyzeBuffer.Length; i += 2, j++)
                {
                    byte dataLeft = mTempBuffer[i + 0];
                    byte dataRight = mTempBuffer[i + 1];
                    double dataMono = ((dataLeft - 128) + (dataRight - 128)) / 2.0 / 128.0;
                    sumVolume += dataMono * dataMono;
                    mAnalyzeBuffer[j] = Window(j, dataMono, mAnalyzeBuffer.Length);
                }
            }
            else if (mSound.WaveFormat.BitsPerSample == 8 && mSound.WaveFormat.Channels == 1)
            {
                for (int i = 0, j = 0; i < mAnalyzeBuffer.Length; i++, j++)
                {
                    double dataMono = (mTempBuffer[i] - 128) / 128.0;
                    sumVolume += dataMono * dataMono;
                    mAnalyzeBuffer[j] = Window(j, dataMono, mAnalyzeBuffer.Length);
                }
            }

            float volume = (float)Math.Sqrt(sumVolume);
            float volumeInDB = 20 * MathF.Log10(Math.Max(volume, 1e-10f));
            _currentSpectrum.TimestampApplication = KWEngine.ApplicationTime;
            _currentSpectrum.TimestampWorld = KWEngine.WorldTime;
            _currentSpectrum.Volume = volumeInDB;
            _currentSpectrum.IsValid = true;

            _fft.RealFFT(mAnalyzeBuffer, true);

            double minFreq = 20;
            double maxFreq = mSound.WaveFormat.SampleRate / 2; // Nyquist
            double logMin = Math.Log10(minFreq);
            double logMax = Math.Log10(maxFreq);
            double maxAmp = 1.0;

            Array.Clear(_magnitudesAddedPerBin);
            Array.Clear(_magnitudesPerBin);
            double sum = 0;
            for (int i = 2; i < mAnalyzeBuffer.Length / 2; i += 2)
            {
                double binFrequency = (i - 0) * mSound.WaveFormat.SampleRate / (double)mAnalyzeBuffer.Length;
                double amp = Math.Sqrt(Math.Pow(mAnalyzeBuffer[i], 2) * Math.Pow(mAnalyzeBuffer[i + 1], 2));
                sum += amp;

                double reference = 1e-6;
                float db = (float)(20 * Math.Log10(Math.Max(amp, reference) / reference));
                //Console.WriteLine(binFrequency + ": " + amp + " (" + db + "dB)");

                if (binFrequency < _binFrequencyMaximums[0])
                {
                    _magnitudesAddedPerBin[0]++;
                    _magnitudesPerBin[0] += amp;
                }
                else
                {
                    for(int j = 0; j < _binFrequencyMaximums.Length - 1; j++)
                    {
                        if (binFrequency >= _binFrequencyMaximums[j] && binFrequency < _binFrequencyMaximums[j + 1])
                        {
                            _magnitudesAddedPerBin[j + 1]++;
                            _magnitudesPerBin[j + 1] += amp;
                            break;
                        }
                    }
                }
            }
            for (int j = 0; j < 20; j++)
            {
                double db = 20 * Math.Log10(Math.Max(_magnitudesPerBin[j] / Math.Max(1, _magnitudesAddedPerBin[j]), 1e-10) / maxAmp);
                _currentSpectrum.ConfigureBand(j, j == 0 ? 0 : _binFrequencyMaximums[j - 1], _binFrequencyMaximums[j], (float)db);
            }
        }

        private int SafeCopyBytes(byte[] source, int sourceOffset, byte[] destination, int destinationOffset, int count)
        {
            int maxSourceBytes = source.Length - sourceOffset;
            int maxDestinationBytes = destination.Length - destinationOffset;
            int bytesToCopy = Math.Min(count, Math.Min(maxSourceBytes, maxDestinationBytes));

            if (bytesToCopy > 0)
            {
                Buffer.BlockCopy(source, sourceOffset, destination, destinationOffset, bytesToCopy);
            }

            return bytesToCopy;
        }

        public string GetFileName()
        {
            return mSound.GetName();
        }

        public bool IsAvailable
        {
            get
            {
                if (_playbackThread == null) return true;
                else if (_playbackThread.IsAlive) return false;
                else return true;
            }
        }

        public bool IsPlayingOrPaused
        {
            get 
            {
                return _playState == PlaybackState.Playing || _playState == PlaybackState.Paused;
            }
        }

        public bool IsPlaying
        {
            get
            {
                return _playState == PlaybackState.Playing;
            }
        }

        public bool IsPaused
        {
            get
            {
                return _playState == PlaybackState.Paused;
            }
        }

        public void Play()
        {
            Stop();

            foreach (int buffer in mBuffers)
                AL.SourceUnqueueBuffer(mSource);

            int buffersFilled = 0;
            foreach (int buffer in mBuffers)
            {
                buffersFilled++;
                if (!StreamIntoBuffer(buffer))
                {
                    break;
                }
            }

            foreach (int buffer in mBuffers)
                AL.SourceQueueBuffer(mSource, buffer);

            State = PlaybackState.Playing;
            AL.SourcePlay(mSource);

            _playbackThread?.Join(1);
            _playbackThread = new Thread(StartAndStreamAudio);
            _playbackThread.Start();
        }

        private void StartAndStreamAudio()
        {
            while (IsPlayingOrPaused)
            {
                int processed;
                AL.GetSource(mSource, ALGetSourcei.BuffersProcessed, out processed);

                while (processed-- > 0)
                {
                    int oldBuffer = AL.SourceUnqueueBuffer(mSource);
                    bool result = StreamIntoBuffer(oldBuffer);
                    if (result)
                    {
                        AL.SourceQueueBuffer(mSource, oldBuffer);
                    }
                    else
                    {
                        if (IsPlaying)
                        {
                            AL.GetSource(mSource, ALGetSourcei.SourceState, out int sState);
                            while (sState == (int)ALSourceState.Playing)
                            {
                                Thread.Sleep(1);
                                AL.GetSource(mSource, ALGetSourcei.SourceState, out sState);
                            }
                            AL.SourceStop(mSource);
                            State = PlaybackState.Stopped;
                        }
                        return;
                    }
                }
                Thread.Sleep(Audio.BufferSizeMs / 2);
            }
            AL.SourceStop(mSource);
            State = PlaybackState.Stopped;
            return;
        }

        public void Stop()
        {
            if (IsPlayingOrPaused)
            {
                State = PlaybackState.Stopped;
                _currentSpectrum.IsValid = false;
            }
            EraseBuffers();
            
            mReadPosition = 0;
        }

        public void Pause()
        {
            if (IsPlaying)
            {
                State = PlaybackState.Paused;
                AL.SourcePause(mSource);
            }
        }

        public void Continue()
        {
            if (IsPaused)
            {
                State = PlaybackState.Playing;
                AL.SourcePlay(mSource);
            }
        }

        public int GetSourceId()
        {
            return mSource;
        }

        public void Clear()
        {
            if(_playbackThread != null)
            {
                while (!_playbackThread.Join(100))
                {
                    KWEngine.LogWriteLine("[Audio] Waiting for audio channel " + mSource + " to stop...");
                }
            }
            
            DeleteBuffers();
            AL.DeleteSource(mSource);
        }

        private void DeleteBuffers()
        {
            foreach (int buffer in mBuffers)
                AL.SourceUnqueueBuffer(mSource);

            foreach (int buffer in mBuffers)
            {
                AL.DeleteBuffer(buffer);
            }
        }
    }
}
