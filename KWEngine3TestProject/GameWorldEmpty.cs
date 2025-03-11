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
    public class GameWorldEmpty : World
    {
        public override void Act()
        {

        }

        public override void Prepare()
        {
            SetCameraPosition(5, 5, 5);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            LightObject p = new LightObject(LightType.Point);
            p.SetPosition(0, 5, 0);
            p.SetColor(1, 1, 1, 2f);
            p.SetNearFar(0.1f, 10f);
            AddLightObject(p);

            Immovable cube1 = new Immovable();
            cube1.IsShadowCaster = false;
            cube1.IsAffectedByLight = true;
            cube1.SetColor(0, 1, 0);
            cube1.SetPosition(3, 0, 0);
            AddGameObject(cube1);

            Immovable cube2 = new Immovable();
            cube2.IsShadowCaster = false;
            cube2.IsAffectedByLight = false;
            cube2.SetColor(1, 0, 0);
            cube2.SetPosition(1, 0, 0);
            AddGameObject(cube2);

            Immovable cube3 = new Immovable();
            cube3.IsShadowCaster = true;
            cube3.IsAffectedByLight = false;
            cube3.SetColor(1, 1, 0);
            cube3.SetPosition(-1, 0, 0);
            AddGameObject(cube3);

            Immovable cube4 = new Immovable();
            cube4.IsShadowCaster = true;
            cube4.IsAffectedByLight = true;
            cube4.SetColor(1, 0, 1);
            cube4.SetPosition(-3, 0, 0);
            AddGameObject(cube4);
        }
    }
}
