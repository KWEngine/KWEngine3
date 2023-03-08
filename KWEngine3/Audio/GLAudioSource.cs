/* Audio (for KWEngine2)
 *  
 * Written by: Lutz Karau <lutz.karau@gmail.com>
 * Licence: GNU LGPL 2.1
 */

using OpenTK.Audio.OpenAL;

namespace KWEngine3.Audio
{
    class GLAudioSource
    {
        private int mSource = -1;
        private string mFileName = "";

        
        public GLAudioSource()
        {
            mSource = AL.GenSource();
        }

        public void SetLooping(bool doLoop)
        {
            IsLooping = doLoop;
        }

        public bool IsLooping { get; private set; } = false;

        public void SetFileName(string name)
        {
            mFileName = name;
        }

        public string GetFileName()
        {
            return mFileName;
        }

        public bool IsPlaying
        {
            get
            {
                return AL.GetSourceState(mSource) == ALSourceState.Playing;
            }
        }

        public void Stop()
        {
            if(IsPlaying)
                AL.SourceStop(mSource);
        }

        public int GetSourceId()
        {
            return mSource;
        }

        public void Clear()
        {
            AL.DeleteSource(mSource);
        }
    }
}
