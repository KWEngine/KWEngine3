using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFirstPersonView;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject
{
    public class GameWorldFBXShadowTest : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Cop", "./Models/Cop/Douglas.glb");

            Immovable cop = new Immovable();
            cop.Name = "Cop";
            cop.SetModel("Cop");
            cop.IsShadowCaster = true;
            cop.SetAnimationID(0);
            cop.IsCollisionObject = true;
            cop.SetAnimationPercentage(0f);

            AddGameObject(cop);

            Immovable floor = new Immovable();
            floor.SetScale(10, 1, 10);
            floor.SetPosition(0, -0.5f, 0);
            floor.IsShadowCaster = true;
            AddGameObject(floor);

            SetCameraPosition(0, 5, 10);
            SetColorAmbient(0.2f, 0.2f, 0.2f);

            LightObjectSun sun = new LightObjectSun(ShadowQuality.High, SunShadowType.Default);
            sun.SetCSMFactor(CSMFactor.Four);
            sun.SetFOV(10);
            sun.SetNearFar(20, 200f);
            sun.SetPosition(50, 50, 50);
            sun.SetColor(1, 1, 1, 3);
            AddLightObject(sun);

            PlayerFirstPerson2 player = new PlayerFirstPerson2();
            player.SetPosition(0, 1, 5);
            player.SetScale(1, 2, 1);
            player.SetRotation(0, 180, 0);
            player.IsCollisionObject = true;
            AddGameObject(player);

            SetCameraToFirstPersonGameObject(player);

            MouseCursorGrab();
        }
    }
}
