using KWEngine3.Helper;
using Microsoft.VisualBasic;
using OpenTK.Audio.OpenAL;
using System.ComponentModel.Design;
using System.Reflection.PortableExecutable;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;

namespace KWEngine3.Audio
{
    internal class GLAudioEngine
    {
        private static Thread mAudioInitThread;
        public string CurrentlyPlaying = null;
        public bool CurrentlyLooping { get; private set; } = false;
        internal static Dictionary<string, CachedSound> CachedSounds { get; private set; } = new Dictionary<string, CachedSound>();

        internal static bool IsInitializing = true;
        private static ALDevice mDeviceID;
        private static ALContext mContext;
        private static bool mAudioOn = false;
        private static List<GLAudioSource> mSources = new List<GLAudioSource>();

        internal const int MAX_CHANNELS = 32;

        private static void TryInitAudio()
        {
            int tries = 0;
            
            while (mAudioOn == false && tries < 10)
            {
                try
                {
                    mDeviceID = ALC.OpenDevice(null);
                    int[] attributes = new int[0];
                    mContext = ALC.CreateContext(mDeviceID, attributes);
                    ALC.MakeContextCurrent(mContext);
                    var version = AL.Get(ALGetString.Version);
                    var vendor = AL.Get(ALGetString.Vendor);
                    
                    if (version == null)
                    {
                        throw new Exception("No Audio devices found");
                    }
                    mAudioOn = true;
                }
                catch (Exception)
                {
                    mAudioOn = false;
                }

                if (mAudioOn == false)
                {
                    tries++;
                    Thread.Sleep(500);
                }
            }
            IsInitializing = false;

            if (mAudioOn)
            {
                for (int i = 0; i < MAX_CHANNELS; i++)
                {
                    GLAudioSource s = new GLAudioSource();
                    mSources.Add(s);
                }
            }
            else
            {
                KWEngine.LogWriteLine("[Audio] No driver available");
            }
        }

        public static void InitAudioEngine()
        {
            foreach (GLAudioSource s in mSources)
            {
                s.Clear();
            }
            mAudioInitThread = new Thread(new ThreadStart(TryInitAudio));
            mAudioInitThread.Start();

        }

        public static void SoundStop(string sound)
        {
            if (mAudioOn)
            {
                GLAudioSource source;
                for (int i = 0; i < mSources.Count; i++)
                {
                    if (mSources[i] != null && mSources[i].IsPlayingOrPaused && sound.Contains(mSources[i].GetFileName()))
                    {
                        source = mSources[i];
                        source.Stop();
                    }
                }
            }
        }

        public static void SoundStop(int channel)
        {
            if (mAudioOn)
            {
                if (mSources[channel] != null && mSources[channel].IsPlayingOrPaused)
                {
                    mSources[channel].Stop();
                }
            }
        }

        public static void SoundStopAll()
        {
            if (mAudioOn)
            {
                for (int i = 0; i < mSources.Count; i++)
                {
                    if (mSources[i] != null && mSources[i].IsPlayingOrPaused)
                    {
                        mSources[i].Stop();
                    }
                }
            }
        }

        public static void SoundChangeGain(string audiofile, float gain)
        {
            int source = FindSourceIdThatIsPlayingAudiofile(HelperGeneral.EqualizePathDividers(audiofile));
            if(source != 1)
            {
                SoundChangeGain(source, gain);
            }
            else
            {
                source = FindSourceIdThatIsPausedOnAudiofile(HelperGeneral.EqualizePathDividers(audiofile));
                if (source != 1)
                {
                    SoundChangeGain(source, gain);
                }
                else
                {
                    KWEngine.LogWriteLine("[Audio] file not playing");
                }
            }
        }

        public static void SoundChangeGain(int channel, float gain)
        {
            if (mAudioOn)
            {
                gain = HelperGeneral.Clamp(gain, 0, 8);
                if (mSources[channel] != null && mSources[channel].IsPlayingOrPaused)
                {
                    AL.Source(mSources[channel].GetSourceId(), ALSourcef.Gain, gain);
                }
            }
        }

        public static void SoundPauseAll()
        {
            if (mAudioOn)
            {
                foreach (GLAudioSource src in mSources)
                {
                    if (src.IsPlaying)
                    {
                        src.Pause();
                    }
                }
            }
        }

        public static void SoundContinueAll()
        {
            if (mAudioOn)
            {
                foreach (GLAudioSource src in mSources)
                {
                    if (src.IsPaused)
                    {
                        src.Continue();
                    }
                }
            }
        }

        public static int FindSourceIdThatIsPlayingAudiofile(string audiofile)
        {
            if (!mAudioOn)
            {
                KWEngine.LogWriteLine("[Audio] device not available");
                return -1;
            }

            for (int i = 0; i < MAX_CHANNELS; i++)
            {
                if (mSources[i].IsPlaying && mSources[i].GetFileName() == audiofile)
                {
                    return mSources[i].GetSourceId();
                }
            }
            return -1;
        }

        public static bool IsSourcePlaying(int src)
        {
            return mSources[src].IsPlaying;
        }

        public static bool IsSourcePaused(int src)
        {
            return mSources[src].IsPaused;
        }

        public static bool IsSourcePlayingOrPaused(int src)
        {
            return mSources[src].IsPlayingOrPaused;
        }

        public static int FindSourceIdThatIsPausedOnAudiofile(string audiofile)
        {
            if (!mAudioOn)
            {
                KWEngine.LogWriteLine("[Audio] device not available");
                return -1;
            }

            for (int i = 0; i < MAX_CHANNELS; i++)
            {
                if (mSources[i].IsPaused && mSources[i].GetFileName() == audiofile)
                {
                    return mSources[i].GetSourceId();
                }
            }
            return -1;
        }

        public static void SoundContinue(int channel)
        {
            if (!mAudioOn)
            {
                KWEngine.LogWriteLine("[Audio] device not available");
                return;
            }

            ALSourceState state = (ALSourceState)AL.GetSource(mSources[channel].GetSourceId(), ALGetSourcei.SourceState);
            if (state == ALSourceState.Paused)
            {
                AL.SourcePlay(mSources[channel].GetSourceId());
            }
            else
            {
                KWEngine.LogWriteLine("[Audio] source already paused or stopped");
            }
        }

        public static void SoundPause(int channel)
        {
            if (!mAudioOn)
            {
                KWEngine.LogWriteLine("[Audio] device not available");
                return;
            }

            ALSourceState state = (ALSourceState)AL.GetSource(mSources[channel].GetSourceId(), ALGetSourcei.SourceState);
            if(state == ALSourceState.Playing)
            {
                AL.SourcePause(mSources[channel].GetSourceId());
            }
            else
            {
                KWEngine.LogWriteLine("[Audio] source already paused or stopped");
            }
        }

        public static int SoundPlay(string sound, bool looping, float volume = 1.0f)
        {
            if (!mAudioOn)
            {
                KWEngine.LogWriteLine("[Audio] device not available");
                return -1;
            }
            volume = volume >= 0 && volume <= 1.0f ? volume : 1.0f;

            CachedSound soundToPlay = null;
            if (CachedSounds.ContainsKey(sound))
            {
                soundToPlay = CachedSounds[sound];
            }
            else
            {
                soundToPlay = new CachedSound(sound);
                if (soundToPlay.AudioData != null)
                {
                    CachedSounds.Add(sound, soundToPlay);
                }
                else
                {
                    KWEngine.LogWriteLine("[Audio] invalid audio file");
                    return -1;
                }
            }

            GLAudioSource source = null;
            int channelNumber = -1;
            for (int i = 0; i < MAX_CHANNELS; i++)
            {
                if (mSources[i].IsAvailable)
                {
                    source = mSources[i];
                    channelNumber = i;
                    break;
                }
            }
            if (source == null)
            {
                KWEngine.LogWriteLine("[Audio] All " + MAX_CHANNELS + " channels are busy playing");
                return -1;
            }
            /*else
            {
                KWEngine.LogWriteLine("picking channel " + channelNumber + " (src id: " + source.GetSourceId() + ") for playback of " + sound + "...");
            }*/
            
            source.SetCachedSound(soundToPlay);
            source.IsLooping = looping;
            source.SetVolume(volume);
            source.Play();

            return channelNumber;
        }



        public static void SoundPreload(string sound)
        {
            if (mAudioOn)
            {
                if (!CachedSounds.ContainsKey(sound))
                {
                    CachedSounds.Add(sound, new CachedSound(sound));
                }
            }
        }

        /// <summary>
        /// Entlädt alle Audioressourcen aus dem Arbeitsspeicher
        /// </summary>
        public static void Dispose()
        {
            if (mAudioOn)
            {
                foreach (GLAudioSource s in mSources)
                {
                    s.Stop();
                    s.Clear();
                }

                if (mContext.Handle != IntPtr.Zero)
                {
                    ALC.MakeContextCurrent(mContext);
                    ALC.DestroyContext(mContext);
                }
                mContext = ALContext.Null;

                if (mDeviceID != IntPtr.Zero)
                {
                    ALC.CloseDevice(mDeviceID);
                }
                mDeviceID = ALDevice.Null;
            }
        }

        public static AudioAnalysis GetAudioAnalysisForChannel(int channel)
        {
            AudioAnalysis a = new AudioAnalysis();
            a.Fill(mSources[channel]._currentSpectrum);
            return a;
        }

        public static bool CheckForNewAudioAnalysisData(int channel, float timestamp)
        {
            return mSources[channel]._currentSpectrum.IsValid && mSources[channel]._currentSpectrum.TimestampWorld > timestamp;
        }
    }
}
