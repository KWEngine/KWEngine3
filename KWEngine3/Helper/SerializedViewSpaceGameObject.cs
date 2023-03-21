using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal class SerializedViewSpaceGameObject
    {
        public string Type { get; set; }
        public string ID { get; set; }
        public string ModelName { get; set; }
        public string ModelPath { get; set; }
        public float[] Position { get; set; }
        public float[] Rotation { get; set; }
        public float[] Scale { get; set; }
    }
}
