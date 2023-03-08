using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal struct ParticleInfo
    {
        public int Texture;
        public int Images;
        public int Samples;

        public ParticleInfo(int texId, int imageCount, int samples)
        {
            Texture = texId;
            Images = imageCount;
            Samples = samples;
        }
    }
}
