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
        /// Gibt an, ob beim Abspielen von Tönen auch die Lautstärke und Frequenzen analysiert werden
        /// </summary>
        public static bool AnalyserActive { get; set; } = false;

        /// <summary>
        /// Gibt die Puffergröße in Millisekunden für alle Audiokanäle an (Minimum: 20ms, Maximum: 100ms, Standardwert: 50ms)
        /// </summary>
        /// <remarks>Es sind nur 10er-Schritte erlaubt (20, 30, usw.)</remarks>
        public static byte BufferSizeMs 
        { 
            get { return _bufferSizeMs; } 
            set 
            {
                if (value < 20)
                    _bufferSizeMs = 20;
                else if(value > 100)
                    _bufferSizeMs = 100;
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
            int source = GLAudioEngine.FindChannelIndexThatIsPlayingAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
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
        /// Erfragt, ob die angegebene Audiodatei gerade pausiert wird
        /// </summary>
        /// <remarks>Sollte ausschließlich bei Audiodateien verwendet werden, die als Loop abgespielt werden!</remarks>
        /// <param name="audiofile">Audiodatei, nach deren Abspielstatus gesucht werden soll</param>
        /// <returns>true, wenn die angegebene Datei gerade pausiert wird</returns>
        public static bool IsAudioFilePaused(string audiofile)
        {
            int channelIndex = GLAudioEngine.FindChannelIndexThatIsPausedOnAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
            return channelIndex != -1;
        }

        /// <summary>
        /// Erfragt, ob die angegebene Audiodatei gerade pausiert wird und gibt ggf. den abspielenden Kanal via out-Parameter zurück
        /// </summary>
        /// <remarks>Sollte ausschließlich bei Audiodateien verwendet werden, die als Loop abgespielt werden!</remarks>
        /// <param name="audiofile">Audiodatei, nach deren Abspielstatus gesucht werden soll</param>
        /// <param name="channelIndex">enthält die ID des Audiokanals, der die Datei gerade abspielt (ist -1, falls die Methode false zurückgibt)</param>
        /// <returns>true, wenn die angegebene Datei gerade pausiert wird</returns>
        public static bool IsAudioFilePaused(string audiofile, out int channelIndex)
        {
            channelIndex = GLAudioEngine.FindChannelIndexThatIsPausedOnAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
            return channelIndex != -1;
        }

        /// <summary>
        /// Erfragt, ob die angegebene Audiodatei gerade abgespielt wird und gibt ggf. den abspielenden Kanal via out-Parameter zurück
        /// </summary>
        /// <remarks>Sollte ausschließlich bei Audiodateien verwendet werden, die als Loop abgespielt werden!</remarks>
        /// <param name="audiofile">Audiodatei, nach deren Abspielstatus gesucht werden soll</param>
        /// <returns>true, wenn die angegebene Datei gerade abgespielt wird</returns>
        public static bool IsAudioFilePlaying(string audiofile)
        {
            int channelIndex = GLAudioEngine.FindChannelIndexThatIsPlayingAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
            return channelIndex != -1;
        }

        /// <summary>
        /// Erfragt, ob die angegebene Audiodatei gerade abgespielt wird
        /// </summary>
        /// <remarks>Sollte ausschließlich bei Audiodateien verwendet werden, die als Loop abgespielt werden!</remarks>
        /// <param name="audiofile">Audiodatei, nach deren Abspielstatus gesucht werden soll</param>
        /// <param name="channelIndex">enthält die ID des Audiokanals, der die Datei gerade abspielt (ist -1, falls die Methode false zurückgibt)</param>
        /// <returns>true, wenn die angegebene Datei gerade abgespielt wird</returns>
        public static bool IsAudioFilePlaying(string audiofile, out int channelIndex)
        {
            channelIndex = GLAudioEngine.FindChannelIndexThatIsPlayingAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
            return channelIndex != -1;
        }

        /// <summary>
        /// Erfragt, ob die angegebene Audiodatei gerade abgespielt oder pausiert wird
        /// </summary>
        /// <remarks>Sollte ausschließlich bei Audiodateien verwendet werden, die als Loop abgespielt werden!</remarks>
        /// <param name="audiofile">Audiodatei, nach deren Abspielstatus gesucht werden soll</param>
        /// <returns>true, wenn die angegebene Datei gerade abgespielt oder pausiert wird</returns>
        public static bool IsAudioFilePlayingOrPaused(string audiofile)
        {
            int channelIndexPlay = GLAudioEngine.FindChannelIndexThatIsPlayingAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
            if(channelIndexPlay < 0)
            {
                int channelIndexPaused = GLAudioEngine.FindChannelIndexThatIsPausedOnAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
                return channelIndexPaused >= 0;
            }
            return true;
        }

        /// <summary>
        /// Erfragt, ob die angegebene Audiodatei gerade abgespielt oder pausiert wird
        /// </summary>
        /// <remarks>Sollte ausschließlich bei Audiodateien verwendet werden, die als Loop abgespielt werden!</remarks>
        /// <param name="audiofile">Audiodatei, nach deren Abspielstatus gesucht werden soll</param>
        /// <param name="channelIndex">enthält die ID des Audiokanals, der die Datei gerade abspielt (ist -1, falls die Methode false zurückgibt)</param>
        /// <returns>true, wenn die angegebene Datei gerade abgespielt oder pausiert wird</returns>
        public static bool IsAudioFilePlayingOrPaused(string audiofile, out int channelIndex)
        {
            channelIndex = -1;
            int channelIndexPlay = GLAudioEngine.FindChannelIndexThatIsPlayingAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
            if (channelIndexPlay < 0)
            {
                int channelIndexPaused = GLAudioEngine.FindChannelIndexThatIsPausedOnAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
                channelIndex = channelIndexPaused;
                return channelIndexPaused >= 0;
            }
            channelIndex = channelIndexPlay;
            return true;
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
            int source = GLAudioEngine.FindChannelIndexThatIsPausedOnAudioFile(HelperGeneral.EqualizePathDividers(audiofile));
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
            if(!AnalyserActive)
            {
                KWEngine.LogWriteLine("[Audio] Global property Audio.AnalyserActive is set to false");
                return new AudioAnalysis();
            }
            else if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
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
            if (!AnalyserActive)
            {
                KWEngine.LogWriteLine("[Audio] Global property Audio.AnalyserActive is set to false");
                return false;
            }
            if (channel < 0 || channel >= GLAudioEngine.MAX_CHANNELS)
            {
                KWEngine.LogWriteLine("[Audio] invalid channel id");
                return false;
            }
            return GLAudioEngine.CheckForNewAudioAnalysisData(channel, timestampWorldOfPreviousData);
        }
    }
}
