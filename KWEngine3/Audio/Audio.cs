using KWEngine3.Helper;

namespace KWEngine3.Audio
{
    /// <summary>
    /// Helfer-Klasse für Audioeffekte
    /// </summary>
    public static class Audio
    {
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
        /// <param name="sourceId">id der Audiospur</param>
        /// <param name="gain">Lautstärke (0.0f bis 1.0f)</param>
        public static void ChangeSoundGain(int sourceId, float gain)
        {
            GLAudioEngine.SoundChangeGain(sourceId, gain);
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
        /// <param name="sourceId">Lanalnummer</param>
        public static void StopSound(int sourceId)
        {
            GLAudioEngine.SoundStop(sourceId);
        }

        /// <summary>
        /// Stoppt alle Töne
        /// </summary>
        public static void StopAllSound()
        {
            GLAudioEngine.SoundStopAll();
        }
    }
}
