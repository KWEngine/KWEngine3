using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class SawSpawnInfo
    {
        public int Health;
        public string UpgradeType;
        public float X;

        private SawSpawnInfo()
        {
            Health = 100;
            UpgradeType = "GunFaster";
        }
        public SawSpawnInfo(int h, string t, float x)
        {
            Health = h;
            UpgradeType = t;
            X = x;
        }
    }
}
