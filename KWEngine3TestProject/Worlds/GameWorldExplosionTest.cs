using KWEngine3;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldExplosionTest : World
    {
        private float[] _timestamps = new float[5];
        
        public override void Act()
        {
            if(WorldTime == 0f)
            {

            }
            else
            {
                for(int i = 0; i < _timestamps.Length; i++)
                {
                    if(WorldTime - _timestamps[i] >= 1f)
                    {
                        ExplosionObject ex = new ExplosionObject(8, 1, 2, 1, ExplosionType.Cube);
                        ex.SetPosition((i - 2) * 3, 0, 0);
                        ex.SetColorEmissive(0, 0, 0);

                        AddExplosionObject(ex);
                        _timestamps[i] = WorldTime;
                    }
                }
            }
        }

        public override void Prepare()
        {
            
        }
    }
}
