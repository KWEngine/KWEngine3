using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldParticleTest
{
    internal class Player : GameObject
    {
        private int _mode = 0;
        private float _velocityY = 0f;
        private const float GRAVITY = 0.001f;
        private float _timestampLastTextureSwitch = 0f;
        private float _timestampLastAttack = 0f;
        private bool _isLookingLeft = false;

        public Player()
        {
            SetModel("KWQuad");
            SetScale(1f, 1f, 1f);
            SetHitboxScale(0.3333f, 1f, 1f);
            SetTexture("./App/Textures/spritesheet.png");
            SetTextureRepeat(1f / 7f, 1f / 3f);
            SetTextureOffset(0, 0);
            HasTransparencyTexture = true;
            SetTextureClip(0.333f, 0.15f);
            IsCollisionObject = true;
            BlendTextureStates = false;
        }

        public override void Act()
        {
            HandleGroundDetection();

            bool isMoving = false;
            if (Keyboard.IsKeyDown(Keys.A))
            {
                MoveAlongVector(LookAtVectorLocalRight, -0.01f);
                isMoving = true;
                _isLookingLeft = true;
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveAlongVector(LookAtVectorLocalRight, +0.01f);
                isMoving = true;
                _isLookingLeft = false;
            }

            if (_mode == 0) // mode 0 = stand on ground
            {
                if (Keyboard.IsKeyPressed(Keys.W))
                {
                    _mode = 1;
                    _velocityY = 0.06f;
                    MoveOffset(0f, _velocityY, 0f);
                }

                if(Keyboard.IsKeyPressed(Keys.Space))
                {
                    _mode = -1; // attack
                    _timestampLastAttack = WorldTime;
                    isMoving = false;

                    FXSlash slash = new FXSlash(Center + LookAtVectorLocalRight);                   
                    CurrentWorld.AddGameObject(slash);

                }
            }
            else if(_mode == 1) // mode 1 = in air
            {
                _velocityY = Math.Max(_velocityY - GRAVITY, -0.5f);
                MoveOffset(0f, _velocityY, 0f);
            }
            else if(_mode == -1)
            {
                if(WorldTime - _timestampLastAttack > 0.5f)
                {
                    _mode = 0;
                }
            }

            HandleCollisions();
            HandleAnimation(isMoving);
        }

        private void HandleAnimation(bool isMoving)
        {
            if (_isLookingLeft)
            {
                SetTextureRepeat(-Math.Abs(TextureRepeat.X), TextureRepeat.Y);
            }
            else
            {
                SetTextureRepeat(Math.Abs(TextureRepeat.X), TextureRepeat.Y);
            }

            if (WorldTime - _timestampLastTextureSwitch > 1f / 10f)
            {
                float textureOffsetX = TextureOffset.X;
                float textureOffsetY = TextureOffset.Y;

                if (_mode == 0)
                {
                    if (isMoving)
                    {
                        textureOffsetY = 1f;
                    }
                    else
                    {
                        textureOffsetY = 0f;
                    }
                    textureOffsetX = (textureOffsetX + 1) % 7;
                }
                else
                {
                    textureOffsetX = 1;
                    textureOffsetY = 1;
                }

                SetTextureOffset(textureOffsetX, textureOffsetY);
                _timestampLastTextureSwitch = WorldTime;
            }
        }

        private void HandleGroundDetection()
        {
            RayIntersectionExtSet result = RaytraceObjectsBelowPosition(RayMode.TwoRays2DPlatformerY, 1f, -0.05f, 0.01f, typeof(Floor));
            if (result.IsValid)
            {
                if (_mode == 0)
                {
                    SetPositionY(result.IntersectionPointNearest.Y, PositionMode.BottomOfAABBHitbox);
                }
                else
                {
                    if (result.DistanceMin <= 0.01f && _velocityY <= 0)
                    {
                        SetPositionY(result.IntersectionPointNearest.Y, PositionMode.BottomOfAABBHitbox);
                        _mode = 0;
                        _velocityY = 0f;
                    }
                }
            }
            else
            {
                if (_mode == 0)
                {
                    _mode = 1;
                    _velocityY = 0f;
                }
            }
        }

        private void HandleCollisions()
        {
            List<Intersection> intersections = GetIntersections<Destructable>();
            foreach (Intersection i in intersections)
            {
                if (_mode == 1)
                {
                    if (i.MTV.Y < 0f)
                    {
                        _velocityY = 0f;
                    }
                }
                MoveOffset(i.MTV);
            }
        }
    }
}
