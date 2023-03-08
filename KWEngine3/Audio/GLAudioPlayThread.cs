/* Audio (for KWEngine2)
 *  
 * Written by: Lutz Karau <lutz.karau@gmail.com>
 * Licence: GNU LGPL 2.1
 */

using System;
using System.Threading;
using System.Threading.Tasks;
using OpenTK.Audio.OpenAL;

namespace KWEngine3.Audio
{
    internal class GLAudioPlayThread
    {
        private CachedSound mData = null;
        private GLAudioSource mSource = null;
        private bool mDoLoop = false;
        private float mVolume = 1.0f;

        internal GLAudioPlayThread(CachedSound data, GLAudioSource source, bool doLoop = false, float volume = 1.0f)
        {
            mData = data;
            mSource = source;
            mDoLoop = doLoop;
            mVolume = volume;
        }

        internal void Play() { 

            AL.Source(mSource.GetSourceId(), ALSourcei.Buffer, mData.GetBufferPointer());   
            if (mDoLoop)
            {
                AL.Source(mSource.GetSourceId(), ALSourceb.Looping, true);
                mSource.SetLooping(true);
            }
            else
            {
                AL.Source(mSource.GetSourceId(), ALSourceb.Looping, false);
                mSource.SetLooping(false);
            }
            AL.Source(mSource.GetSourceId(), ALSourcef.Gain, mVolume);
            mSource.SetFileName(mData.GetName());
            AL.SourcePlay(mSource.GetSourceId());            

            mSource.SetFileName("");
            mSource.SetLooping(false);
        }
    }
}
