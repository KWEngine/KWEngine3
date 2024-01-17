using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3
{
    internal class KeyboardExtState
    {
        public float Time { get; set; }
        public bool OldWorld { get; set; }

        public void SwitchToOldWorld()
        {
            OldWorld = true;
        }
    }
}
