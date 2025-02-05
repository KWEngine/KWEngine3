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
            //KWEngine.LoadModel("Tower", "./Models/WhiteClown/WhiteClown.gltf");
            KWEngine.LoadModel("Tower", "./Models/knezi1.glb");
            SetCameraPosition(-3, 1.5f * 5, 10 * 1);
            SetCameraTarget(0, 1.5f, 0);

            Immovable t = new Immovable();
            t.SetModel("Tower");
            //t.SetHitboxToCapsule();
            t.SetAnimationID(0);
            t.SetScale(1.5f);
            //t.SetHitboxScale(0.45f, 1f, 1.5f);
            t.SetHitboxScale(1f);
            t.SetAnimationPercentage(0);
            t.IsCollisionObject = true;
            AddGameObject(t);

            Pointer p = new Pointer();
            p.SetModel("KWSphere");
            p.SetScale(0.1f);
            p.SetColor(1, 0, 0);
            p.SetColorEmissive(1, 0, 0, 2);
            AddGameObject(p);
        }
    }
}
