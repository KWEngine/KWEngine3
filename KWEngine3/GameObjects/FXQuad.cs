using KWEngine3.Helper;

namespace KWEngine3.GameObjects
{
    internal sealed class FXQuad : RenderObject
    {
        public FXQuad(ParticleType type, ParticleObjectAnchor anchor)
        {
            GetTypeInfo(type, out int columns, out int rows, out int samplesAvailable, out bool isBurst, out int textureId, out string textureName);

            _framesPlayed = 0;
            _columns = columns;
            _rows = rows;
            _framesTotal = samplesAvailable;
            _loop = !isBurst;
            _lengthSeconds = 5f;
            _removeAfter = true;
            _active = false;
            _faceCam = true;
            _speed = SPEED_060FPS;

            SetModel(anchor == ParticleObjectAnchor.Center ? "KWQuad" : "KWQuad2D");
            IsAffectedByLight = false;
            HasTransparencyTexture = true;
            BlendTextureStates = false;

            SetTexture(textureId, textureName, TextureType.Albedo, 0);
            SetTexture(textureId, textureName, TextureType.Emissive, 0);
            SetTextureRepeat(1f / _columns, 1f / _rows);

            SetColorEmissive(0, 0, 0, 0.0f);

            _timestamp = KWEngine.WorldTime;
        }

        public FXQuad(string texture, int columns, int rows, ParticleObjectAnchor anchor)
        {
            _columns = columns;
            _rows = rows;
            _framesTotal = _columns * _rows;
            _loop = false;
            _lengthSeconds = 5f;
            _removeAfter = true;
            _active = false;
            _faceCam = true;
            _speed = SPEED_060FPS;

            SetModel(anchor == ParticleObjectAnchor.Center ? "KWQuad" : "KWQuad2D");
            IsAffectedByLight = false;
            HasTransparencyTexture = true;
            BlendTextureStates = false;

            SetTexture(texture, TextureType.Albedo);
            SetTexture(texture, TextureType.Emissive);
            SetTextureRepeat(1f / _columns, 1f / _rows);

            SetColorEmissive(0, 0, 0, 0.0f);

            _timestamp = KWEngine.WorldTime;
        }

        /// <summary>
        /// Setzt die Dauer der Loop-Partikel
        /// </summary>
        /// <param name="durationInSeconds">Dauer (in Sekunden)</param>
        public void SetDuration(float durationInSeconds)
        {
            if (_loop)
                _lengthSeconds = durationInSeconds > 0 ? durationInSeconds : 5f;
            else
            {
                KWEngine.LogWriteLine("[ParticleObject] Cannot set duration on non-looping particles.");
            }
        }

        public bool IsPlaying => _active;

        public void SetFaceCameraAlways(bool faceCam)
        {
            _faceCam = faceCam;
        }
        public void SetSpriteSheetLooping(bool isLooping)
        {
            _loop = isLooping;
        }

        public void SetSpriteSheetEmissiveLevel(float emissive)
        {
            SetColorEmissive(0f, 0f, 0f, Math.Clamp(emissive, 0f, 2f));
        }

        public void SetSpriteSheetSpeed(ParticleObjectSpeed s)
        {
            _speed = s switch
            {
                ParticleObjectSpeed.FPS030 => SPEED_030FPS,
                ParticleObjectSpeed.FPS060 => SPEED_060FPS,
                ParticleObjectSpeed.FPS120 => SPEED_120FPS,
                _ => SPEED_060FPS
            };
        }

        public void Start()
        {
            _timestampStart = KWEngine.WorldTime;
            _active = true;
            SetOpacity(1f);
        }

        public void Stop()
        {
            _active = false;
        }

        public void Rewind()
        {
            _framesPlayed = 0;
        }

        public void SetRotationAfterTurn(float angle)
        {
            _rotationAfterTurn = angle % 360f;
        }

        public override void Act()
        {
            if (!_active || _framesPlayed >= _framesTotal)
            {
                return;
            }

            if(_faceCam)
            {
                TurnTowardsXYZ(CurrentWorld.CameraPosition);
                AddRotationZ(_rotationAfterTurn);
            }

            if (KWEngine.WorldTime - _timestamp >= _speed)
            {
                _framesPlayed++;
                _offsetX++;
                if (_offsetX == _columns || _framesPlayed >= _framesTotal)
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


                if (!_loop)
                {
                    if (_framesTotal > 1)
                    {
                        float progress = _framesPlayed / _framesTotal;
                        if (progress >= 0.75f)
                        {
                            float fadeProgress = (progress - 0.75f) / 0.25f;
                            float opacity = 1f - Math.Clamp(fadeProgress, 0f, 1f);
                            SetOpacity(opacity);
                        }
                        else
                        {
                            SetOpacity(1f);
                        }
                    }

                    if (_framesPlayed >= _framesTotal)
                    {
                        Stop();
                        if (_removeAfter)
                        {
                            SkipRender = true;
                            KWEngine.CurrentWorld.RemoveRenderObject(this);
                        }
                    }
                }
                else
                {
                    if (_framesPlayed >= _framesTotal)
                        Rewind();

                    float secondsPlayed = KWEngine.WorldTime - _timestampStart;
                    float timeNormalized = HelperGeneral.NormalizeValueToRange(secondsPlayed, 0f, _lengthSeconds);
                    if(timeNormalized > 0.9f)
                    {
                        float opacity = 1f - Math.Clamp((timeNormalized - 0.9f) / 0.1f, 0f, 1f);
                        SetOpacity(opacity);
                        
                    }
                    if(timeNormalized >= 1f)
                    {
                        Stop();
                        SkipRender = true;
                        KWEngine.CurrentWorld.RemoveRenderObject(this);
                    }

                }
            }
        }

        #region Internals
        private const float SPEED_030FPS = 1f / 30f;
        private const float SPEED_060FPS = 1f / 60f;
        private const float SPEED_120FPS = 1f / 120f;

        private int _columns;
        private int _rows;
        private int _offsetX = 0;
        private int _offsetY = 0;
        private float _framesPlayed;
        private float _timestamp;
        private float _speed;
        private bool _loop;
        private float _framesTotal;
        private bool _removeAfter;
        private bool _active;
        private bool _faceCam;
        private float _lengthSeconds;
        private float _timestampStart;
        private float _rotationAfterTurn;

        internal void GetTypeInfo(ParticleType type, out int columns, out int rows, out int samplesAvailable, out bool isBurst, out int textureId, out string textureName)
        {
            isBurst = true;
            if (type == ParticleType.LoopSmoke1 || type == ParticleType.LoopSmoke2 || type == ParticleType.LoopSmoke3)
            {
                isBurst = false;
            }

            ParticleInfo info = KWEngine.ParticleDictionary[type];
            columns = info.Images;
            rows = info.Images;
            samplesAvailable = info.Samples;
            textureId = info.Texture;
            textureName = info.File;
        }
        #endregion
    }
}
