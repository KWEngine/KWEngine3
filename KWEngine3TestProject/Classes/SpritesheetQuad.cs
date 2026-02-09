using Assimp;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;

namespace SpriteSheetQuad
{
    public class SpritesheetQuad : RenderObject
    {
        public enum Speed
        {
            FPS120,
            FPS060,
            FPS030
        }

        public enum Anchor
        {
            Center,
            Bottom
        }

        private const float SPEED_030FPS = 0.033333f;
        private const float SPEED_060FPS = 0.016666f;
        private const float SPEED_120FPS = 0.008333f;

        private int _columns;
        private int _rows;
        private int _offsetX = 0;
        private int _offsetY = 0;
        private float _framesPlayed = 0;
        private float _timestamp;
        private float _speed = SPEED_060FPS;
        private bool _loop;
        private float _framesTotal;

        public SpritesheetQuad(string texture, int columns, int rows, Anchor anchor = Anchor.Center)
        {
            _columns = columns;
            _rows = rows;
            _framesTotal = _columns * _rows;
            _loop = false;

            SetModel(anchor == Anchor.Center ? "KWQuad" : "KWQuad2D");
            IsAffectedByLight = false;
            HasTransparencyTexture = true;
            BlendTextureStates = false;

            SetTexture(texture, KWEngine3.TextureType.Albedo);
            SetTexture(texture, KWEngine3.TextureType.Emissive);
            SetTextureRepeat(1f / _columns, 1f / _rows);

            SetColorEmissive(0, 0, 0, 0.0f);

            _timestamp = KWEngine.WorldTime;
        }

        public void SetSpriteSheetLooping(bool isLooping)
        {
            _loop = isLooping;
        }

        public void SetSpriteSheetEmissiveLevel(float emissive)
        {
            SetColorEmissive(0f, 0f, 0f, Math.Clamp(emissive, 0f, 2f));
        }

        public void SetSpriteSheetSpeed(Speed s)
        {
            _speed = s switch
            {
                Speed.FPS030 => SPEED_030FPS,
                Speed.FPS060 => SPEED_060FPS,
                Speed.FPS120 => SPEED_120FPS,
                _ => SPEED_060FPS
            };
        }

        public override void Act()
        {
            if(KWEngine.WorldTime - _timestamp >= _speed)
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

                if(!_loop)
                {
                    float o = HelperVector.SmoothStep(0.5f, 1.0f, _framesPlayed / _framesTotal);
                    SetOpacity(1f - o);

                    if (_framesPlayed >= _framesTotal)
                    {
                        CurrentWorld.RemoveRenderObject(this);
                    }
                }
            }
        }
    }
}
