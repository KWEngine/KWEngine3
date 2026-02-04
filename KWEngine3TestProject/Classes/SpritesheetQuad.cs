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
    internal class SpritesheetQuad : RenderObject
    {
        private int _columns;
        private int _rows;
        private int _offsetX = 0;
        private int _offsetY = 0;
        private float _framesPlayed = 0;
        private float _timestamp;

        public SpritesheetQuad(string texture, int columns, int rows)
        {
            _columns = columns;
            _rows = rows;

            SetModel("KWQuad");
            IsAffectedByLight = false;
            HasTransparencyTexture = true;
            BlendTextureStates = false;
            SetTexture(texture, KWEngine3.TextureType.Albedo);

            SetTextureRepeat(1f / _columns, 1f / _rows);

            _timestamp = KWEngine.WorldTime;
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

                float o = HelperVector.SmoothStep(0.9f, 1.0f, _framesPlayed / (_columns * _rows));
                SetOpacity(1f - o);

                if (_framesPlayed >= _columns * _rows)
                    CurrentWorld.RemoveRenderObject(this);
            }
        }
    }
}
