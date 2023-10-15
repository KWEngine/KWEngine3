using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldTutorial
{
    internal class SawPowerUp : GameObject
    {
        private float _intensity = 0f;
        private float _counter = 0f;

        public SawPowerUp()
        {
            SetModel("KWQuad");
            HasTransparencyTexture = true;
            SetColorEmissive(1, 1, 0, _intensity);
        }

        public override void Act()
        {
            
            _counter = (_counter + (MathF.PI * 2 / 240f)) % (MathF.PI * 2);
            SetColorEmissive(1, 1, 0, MathF.Sin(_counter) + 1f);
        }
    }
}
