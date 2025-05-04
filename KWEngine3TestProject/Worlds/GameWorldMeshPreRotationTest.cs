using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldGLTFTest;
using KWEngine3.Helper;
using System;
using System.Collections.Generic;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldPlatformerPack;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldMeshPreRotationTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Scooter", "./Models/GLTFTest/Vespa.glb");
            KWEngine.LoadModel("Car", "./Models/GLTFTest/CarLowPoly.gltf");
            KWEngine.LoadModel("Car2", "./Models/GLTFTest/CarCartoon.gltf");
            KWEngine.LoadModel("Car3", "./Models/GLTFTest/CarSports.gltf");

            SetCameraPosition(5f, 5f, 5f);
            SetCameraTarget(0f, 0f, 0f);
            SetColorAmbient(0.25f, 0.25f, 0.25f);
            SetBackgroundSkybox("./Textures/skybox.dds");
            SetBackgroundBrightnessMultiplier(4);

            LightObject sun = new LightObjectSun(ShadowQuality.High);
            sun.Name = "Sun";
            sun.SetPosition(100, 100, 100);
            sun.SetNearFar(30, 100);
            sun.SetFOV(40);
            sun.SetColor(1, 1, 1, 2.5f);
            AddLightObject(sun);

            Floor f = new Floor();
            f.SetScale(100, 1, 100);
            f.SetPosition(0, -0.5f, 0);
            f.IsShadowCaster = true;
            f.IsCollisionObject = true;
            f.SetTextureRepeat(25, 25);
            f.SetTexture("./Textures/sand_diffuse.dds", TextureType.Albedo);
            f.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            AddGameObject(f);

            Scooter s = new Scooter();
            s.SetModel("Car3");
            s.IsShadowCaster = true;
            s.SetPosition(0, 0, 0);
            s.SetScale(3);
            s.IsCollisionObject = true;
            s.SetRotation(0, 180, 0);
            s.UpdateLast = true;
            AddGameObject(s);
        }
    }

    internal class Scooter : GameObject
    {
        private float _degrees = 0f;
        private float _degreesWheels = 0f;
        private float _degreesHandleBar = 0f;
        private float _velocity = 0f;
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.F1))
            {
                SetModel("Car");
            }
            if (Keyboard.IsKeyPressed(Keys.F2))
            {
                SetModel("Scooter");
            }

            if (Keyboard.IsKeyDown(Keys.W))
            {
                if(_velocity == 0f)
                {
                    _velocity = 0.0001f;
                }
                else
                {
                    _velocity = MathHelper.Clamp(_velocity * 1.0125f, 0f, 0.03f);
                }
            }
            else if(Keyboard.IsKeyDown(Keys.S))
            {
                _velocity = MathHelper.Clamp(_velocity - 0.0005f, 0f, 0.03f);
            }
            else
            {
                _velocity = MathHelper.Clamp(_velocity - 0.00001f, 0f, 0.03f);
            }


            if (Keyboard.IsKeyDown(Keys.D) || Keyboard.IsKeyDown(Keys.A))
            {
                if (Keyboard.IsKeyDown(Keys.A))
                {
                    _degrees = _degrees + 0.01f * (_velocity * 500);
                    _degreesHandleBar = MathHelper.Clamp(_degreesHandleBar + 0.1f, -10, 10);
                }
                if (Keyboard.IsKeyDown(Keys.D))
                {
                    _degrees = _degrees - 0.01f * (_velocity * 500);
                    _degreesHandleBar = MathHelper.Clamp(_degreesHandleBar - 0.1f, -10, 10);
                }
            }
            else
            {
                if(_degreesHandleBar < 0)
                {
                    _degreesHandleBar = MathHelper.Clamp(_degreesHandleBar + 0.2f, -10, 0);
                }
                else
                {
                    _degreesHandleBar = MathHelper.Clamp(_degreesHandleBar - 0.2f, 0, 10);
                }
            }

            _degreesWheels += _velocity;
            if (this.GetModelName() == "Scooter")
            {
                HelperRotation.SetMeshPreRotationYZX(this, 1, _degreesWheels * 10, _degreesHandleBar, 0); // WheelFront
                HelperRotation.SetMeshPreRotationYZX(this, 2, _degreesWheels * 10, 0, 0); // WheelBack
                HelperRotation.SetMeshPreRotationYZX(this, 3, 0, _degreesHandleBar, 0); // HandleBar
                HelperRotation.SetMeshPreRotationYZX(this, 4, 0, _degreesHandleBar, 0); // WheelHood
            }
            else if(this.GetModelName() == "Car")
            {
                HelperRotation.SetMeshPreRotationYZX(this, 2, _degreesWheels * 50, _degreesHandleBar * 2, 0); // WheelFront
                HelperRotation.SetMeshPreRotationYZX(this, 4, _degreesWheels * 50, _degreesHandleBar * 2, 0); // WheelFront
                HelperRotation.SetMeshPreRotationYZX(this, 3, _degreesWheels * 50, 0, 0); // WheelBack
            }
            else if (this.GetModelName() == "Car2")
            {
                HelperRotation.SetMeshPreRotationYZX(this, 1, _degreesWheels * 50, 0, 0); // WheelBack
                HelperRotation.SetMeshPreRotationYZX(this, 2, _degreesWheels * 50, _degreesHandleBar * 2, 0); // WheelFrontRight
                HelperRotation.SetMeshPreRotationYZX(this, 3, _degreesWheels * 50, _degreesHandleBar * 2, 0); // WheelFrontLeft
            }
            else if (this.GetModelName() == "Car3")
            {
                HelperRotation.SetMeshPreRotationYZX(this, 3, _degreesWheels * 50, _degreesHandleBar * 2, 0); // WheelFrontLeft
                HelperRotation.SetMeshPreRotationYZX(this, 2, _degreesWheels * 50, _degreesHandleBar * 2, 0); // WheelFrontRight
                HelperRotation.SetMeshPreRotationYZX(this, 1, _degreesWheels * 50, 0, 0); // WheelBack
            }
            
            SetRotation(0, _degrees, 0);
            MoveAlongVector(LookAtVector, _velocity);
            UpdateSunAndCamera();
        }

        private void UpdateSunAndCamera()
        {
            CurrentWorld.SetCameraPosition(this.Position.X - this.LookAtVectorLocalRight.X * 3, 2f, this.Position.Z - this.LookAtVector.Z * 3);
            //CurrentWorld.SetCameraPosition(this.Position.X - this.LookAtVector.X * 5, 2f, this.Position.Z - this.LookAtVector.Z * 5);
            CurrentWorld.SetCameraTarget(this.Position.X, this.Position.Y, this.Position.Z);

            LightObject sun = CurrentWorld.GetLightObjectByName("Sun");
            if(sun != null)
            {
                sun.SetPosition(this.Position + new Vector3(25, 25, 25));
                sun.SetTarget(this.Position);
            }
        }
    }
}
