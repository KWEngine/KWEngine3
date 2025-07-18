using KWEngine3.Audio.Lomont;
using KWEngine3.Helper;

namespace KWEngine3.Audio
{
    /// <summary>
    /// Helfer-Klasse für Audioeffekte
    /// </summary>
    public static class Audio
    {
        internal static byte _bufferSizeMs = 50;

        /// <summary>
        /// Gibt die Puffergröße in Millisekunden für alle Audiokanäle an (Minimum: 20ms, Standardwert: 50ms)
        /// </summary>
        /// <remarks>Es sind nur 10er-Schritte erlaubt (20, 30, usw.)</remarks>
        public static byte BufferSizeMs 
        { 
            get { return _bufferSizeMs; } 
            set 
            {
                if (value < 20)
                    _bufferSizeMs = 20;
                else 
                {
                    _bufferSizeMs = (byte)((value + 9) / 10 * 10);
                }
                    
            }
        }

        /// <summary>
        /// Spielt einen Ton ab
        /// </summary>
        /// <param name="audiofile">Audiodatei</param>
        /// <param name="playLooping">looped playback?</param>
        /// <param name="volume">Lautstärke</param>
        /// <returns>ID des verwendeten Audiokanals</returns>
        public static int PlaySound(string audiofile, bool playLooping = false, float volume = 1.0f)
        {
            audiofile = HelperGeneral.EqualizePathDividers(audiofile);
            return GLAudioEngine.SoundPlay(audiofile, playLooping, volume);
        }

        /// <summary>
        /// Ändert die Lautstärke eines Tons
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        /// <param name="gain">Lautstärke (0.0f bis 1.0f)</param>
        public static void ChangeSoundGain(int channel, float gain)
        {
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
            {
                KWEngine.LogWriteLine("[Audio] invalid channel id");
                return;
            }
            GLAudioEngine.SoundChangeGain(channel, gain);
        }

        /// <summary>
        /// Stoppt einen Ton
        /// </summary>
        /// <param name="audiofile">Audiodatei</param>
        public static void StopSound(string audiofile)
        {
            audiofile = HelperGeneral.EqualizePathDividers(audiofile);
            GLAudioEngine.SoundStop(audiofile);
        }

        /// <summary>
        /// Lädt eine Audiodatei in den Arbeitsspeicher
        /// </summary>
        /// <param name="audiofile">Audiodatei</param>
        public static void PreloadSound(string audiofile)
        {
            audiofile = HelperGeneral.EqualizePathDividers(audiofile);
            GLAudioEngine.SoundPreload(audiofile);
        }

        /// <summary>
        /// Stoppt den angegebenen Audiokanal
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        public static void StopSound(int channel)
        {
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
            {
                KWEngine.LogWriteLine("[Audio] invalid channel id");
                return;
            }
            GLAudioEngine.SoundStop(channel);
        }

        /// <summary>
        /// Pausiert den ersten Audiokanal, der die angegebene Datei gerade abspielt
        /// </summary>
        /// <param name="audiofile">zu pausierende Audiodatei</param>
        public static void PauseSound(string audiofile)
        {
            int source = GLAudioEngine.FindSourceIdThatIsPlayingAudiofile(HelperGeneral.EqualizePathDividers(audiofile));
            if(source != -1)
            {
                PauseSound(source);
            }
            else
            {
                KWEngine.LogWriteLine("[Audio] file is not playing");
            }
        }

        /// <summary>
        /// Pausiert den angegebenen Audiokanal
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        public static void PauseSound(int channel)
        {
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
            {
                KWEngine.LogWriteLine("[Audio] invalid channel id");
                return;
            }
            GLAudioEngine.SoundPause(channel);
        }

        /// <summary>
        /// Erfragt, ob der angegebene Audiokanal gerade etwas abspielt
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        /// <returns>true, wenn der Kanal abgespielt wird</returns>
        public static bool IsChannelPlaying(int channel)
        {
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
                return false;
            return GLAudioEngine.IsSourcePlaying(channel);
        }

        /// <summary>
        /// Erfragt, ob der angegebene Audiokanal gerade pausiert ist
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        /// <returns>true, wenn der Kanal pausiert ist</returns>
        public static bool IsChannelPaused(int channel)
        {
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
                return false;
            return GLAudioEngine.IsSourcePaused(channel);
        }

        /// <summary>
        /// Erfragt, ob der angegebene Audiokanal gerade etwas abspielt oder pausiert ist
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        /// <returns>true, wenn der Kanal abgespielt wird oder pausiert ist</returns>
        public static bool IsChannelPlayingOrPaused(int channel)
        {
            if(channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
                return false;
            return GLAudioEngine.IsSourcePlayingOrPaused(channel);
        }

        /// <summary>
        /// Setzt die Wiedergabe der angegebenen Audiodatei fort
        /// </summary>
        /// <param name="audiofile">fortzusetzende Audiodatei</param>
        public static void ContinueSound(string audiofile)
        {
            int source = GLAudioEngine.FindSourceIdThatIsPausedOnAudiofile(HelperGeneral.EqualizePathDividers(audiofile));
            if(source != -1)
            {
                ContinueSound(source);
            }
            else
            {
                KWEngine.LogWriteLine("[Audio] file is not paused");
            }
        }

        /// <summary>
        /// Setzt die Wiedergabe des Audiokanals fort
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        public static void ContinueSound(int channel)
        {
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
            {
                KWEngine.LogWriteLine("[Audio] invalid channel id");
                return;
            }
            GLAudioEngine.SoundContinue(channel);
        }

        /// <summary>
        /// Pausiert alle gerade wiedergegebenen Töne
        /// </summary>
        public static void PauseAllSound()
        {
            GLAudioEngine.SoundPauseAll();
        }

        /// <summary>
        /// Setzt die Wiedergabe aller gerade pausierten Töne fort
        /// </summary>
        public static void ContinueAllSound()
        {
            GLAudioEngine.SoundContinueAll();
        }

        /// <summary>
        /// Stoppt alle Töne
        /// </summary>
        public static void StopAllSound()
        {
            GLAudioEngine.SoundStopAll();
        }

        /// <summary>
        /// Ruft die spektrografische Analysedaten für den angegebenen (gerade spielenden) Kanal ab
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        /// <returns>Analysedaten</returns>
        public static AudioAnalysis GetAudioAnalysisForChannel(int channel)
        {
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
            {
                KWEngine.LogWriteLine("[Audio] invalid channel id");
                return new AudioAnalysis();
            }

            return GLAudioEngine.GetAudioAnalysisForChannel(channel);
        }

        /// <summary>
        /// Prüft, ob neue Analysedaten für den gegebenen Kanal verfügbar sind
        /// </summary>
        /// <param name="channel">Kanalnummer</param>
        /// <param name="timestampWorldOfPreviousData">Zeitstempel (Weltzeit) der vorherigen Daten</param>
        /// <returns></returns>
        public static bool IsNewAudioAnalysisDataAvailable(int channel, float timestampWorldOfPreviousData)
        {
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
            {
                KWEngine.LogWriteLine("[Audio] invalid channel id");
                return false;
            }
            return GLAudioEngine.CheckForNewAudioAnalysisData(channel, timestampWorldOfPreviousData);
        }
    }
}
