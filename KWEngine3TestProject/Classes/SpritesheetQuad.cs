using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes
{
    internal class SpritesheetQuad : GameObject
    {
        private int _columns;
        private int _rows;
        private int _offsetX = 0;
        private int _offsetY = 0;
        private float _framesPlayed = 0;
        private float _timestamp;
        private LightObjectPoint l;

        public SpritesheetQuad(string texture, int columns, int rows)
        {
            _columns = columns;
            _rows = rows;

            SetModel("KWQuad2D");
            IsAffectedByLight = false;
            HasTransparencyTexture = true;
            BlendTextureStates = false;
            SetColorEmissive(1, 0.5f, 0, 0.05f);
            SetTexture(texture, KWEngine3.TextureType.Albedo);
            SetTextureRepeat(1f / _columns, 1f / _rows);

            _timestamp = KWEngine.WorldTime;

            l = new LightObjectPoint(ShadowQuality.NoShadow);
            CurrentWorld.AddLightObject(l);
        }

        public void SetPosition(float x, float y, float z)
        {
            base.SetPosition(x, y, z);
            l.SetPosition(x, y + 2.5f, z);
            l.SetNearFar(0, 7);
            l.SetColor(1, 0.5f, 0.25f, 5);
        }

        public override void Act()
        {
            if(KWEngine.WorldTime - _timestamp > 0.0166666f)
            {
                _framesPlayed++;
                _offsetX++;
                if (_offsetX == _columns)
                {
                    _offsetX = 0;
                    _offsetY++;
                    if (_offsetY == _rows)
                    {
                        _offsetY = 0;
                    }
                }

                SetTextureOffset(_offsetX, _offsetY);
                _timestamp = KWEngine.WorldTime;

                float framesTotal = _columns * _rows;
                float lightIntensity = -framesTotal * 0.5f * ((_framesPlayed - framesTotal * 0.5f) / framesTotal) + 5f;


                float o = HelperVector.SmoothStep(0.9f, 1.0f, _framesPlayed / (_columns * _rows));
                SetOpacity(1f - o);

                l.SetColor(l.Color.X, l.Color.Y, l.Color.Z, lightIntensity);

                if (_framesPlayed >= _columns * _rows)
                {
                    CurrentWorld.RemoveGameObject(this);
                    CurrentWorld.RemoveLightObject(l);
                }
            }
        }
    }
}
