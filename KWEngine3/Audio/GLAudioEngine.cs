using OpenTK.Audio.OpenAL;
using KWEngine3.Helper;

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

        private static int mChannels = 32;

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
                        throw new Exception("No Audio devices found.");
                    }
                    //KWEngine.LogWriteLine("[Audio] Initialized successfully");
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
                for (int i = 0; i < mChannels; i++)
                {
                    GLAudioSource s = new GLAudioSource();
                    mSources.Add(s);
                }
            }
            else
            {
                KWEngine.LogWriteLine("[Audio] No driver available :-(");
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
                    if (mSources[i] != null && mSources[i].IsPlaying && sound.Contains(mSources[i].GetFileName()))
                    {
                        source = mSources[i];
                        source.Stop();
                    }
                }
            }
        }

        public static void SoundStop(int sourceId)
        {
            if (mAudioOn)
            {
                if (mSources[sourceId] != null && mSources[sourceId].IsPlaying)
                {
                    mSources[sourceId].Stop();
                }
            }
        }

        public static void SoundStopAll()
        {
            if (mAudioOn)
            {
                GLAudioSource source;
                for (int i = 0; i < mSources.Count; i++)
                {
                    if (mSources[i] != null && mSources[i].IsPlaying)
                    {
                        source = mSources[i];
                        source.Stop();
                    }
                }
            }
        }

        public static void SoundChangeGain(int sourceId, float gain)
        {
            if (mAudioOn)
            {
                gain = HelperGeneral.Clamp(gain, 0, 8);
                if (mSources[sourceId] != null && mSources[sourceId].IsPlaying)
                {
                    AL.Source(mSources[sourceId].GetSourceId(), ALSourcef.Gain, gain);
                }
            }
        }

        public static int SoundPlay(string sound, bool looping, float volume = 1.0f)
        {
            if (!mAudioOn)
            {
                KWEngine.LogWriteLine("Error playing audio: audio device not available.");
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
                CachedSounds.Add(sound, soundToPlay);
            }

            GLAudioSource source = null;
            int channelNumber = -1;
            for (int i = 0; i < mChannels; i++)
            {
                if (!mSources[i].IsPlaying)
                {
                    source = mSources[i];
                    channelNumber = i;
                    source.SetFileName(sound);
                    break;
                }
            }
            if (source == null)
            {
                KWEngine.LogWriteLine("Error playing audio file: all " + mChannels + " channels are busy.");
                return -1;
            }

            AL.Source(source.GetSourceId(), ALSourcei.Buffer, soundToPlay.GetBufferPointer());
            if (looping)
            {
                AL.Source(source.GetSourceId(), ALSourceb.Looping, true);
                source.SetLooping(true);
            }
            else
            {
                AL.Source(source.GetSourceId(), ALSourceb.Looping, false);
                source.SetLooping(false);
            }
            AL.Source(source.GetSourceId(), ALSourcef.Gain, volume);
            AL.SourcePlay(source.GetSourceId());
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
    }
}
