using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject
{
    public class GameWorldRaycasting : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            //KWEngine.LoadModel("Tower", "./Models/Towers/tower05.fbx");
            //KWEngine.LoadModel("Tower", "./Models/robotERS.fbx");
            KWEngine.LoadModel("Tower", "./Models/PlatformerPack/Toon.glb");
            SetCameraPosition(0, 75, 50);
            SetCameraTarget(0, 25, 0);

            Immovable t = new Immovable();
            t.SetModel("Tower");
            t.SetAnimationID(0);
            t.SetScale(50);
            t.SetHitboxScale(0.1f);
            t.SetAnimationPercentage(0);
            t.IsCollisionObject = true;
            AddGameObject(t);

            Pointer p = new Pointer();
            p.SetModel("KWSphere");
            p.SetColor(1, 0, 0);
            p.SetColorEmissive(1, 0, 0, 2);
            AddGameObject(p);
        }
    }
}
