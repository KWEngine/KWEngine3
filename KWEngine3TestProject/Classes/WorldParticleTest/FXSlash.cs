using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldParticleTest
{
    internal class FXSlash : GameObject
    {
        private const int FIELDCOUNTX = 4;
        private const int FIELDCOUNTY = 4;

        private float _spawnTime = 0f;
        private int _counter = 0;

        public FXSlash(Vector3 spawnPosition)
        {
            _spawnTime = KWEngine.WorldTime;

            SetModel("KWQuad");
            SetTexture("./Textures/slashtest.png");
            SetTextureRepeat(1f / FIELDCOUNTX, 1f / FIELDCOUNTY);
            SetTextureOffset(0, 0);
            HasTransparencyTexture = true;
            SetPosition(spawnPosition + Vector3.UnitZ);
            SetScale(2);
            SetHue(HelperRandom.GetRandomNumber(0f, 359f));
        }

        public override void Act()
        {
            if(WorldTime - _spawnTime > 0.015f)
            {
                _counter++;
                _spawnTime = WorldTime;

                if(_counter > 15)
                {
                    CurrentWorld.RemoveGameObject(this);
                    //CurrentWorld.RemoveRenderObject(this);
                    return;
                }
            }
            SetTextureOffset(_counter % 4, _counter / 4);

        }
    }
}
