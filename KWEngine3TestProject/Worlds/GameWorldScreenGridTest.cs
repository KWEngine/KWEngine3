using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFoliageTest;
using KWEngine3TestProject.Classes.WorldScreenGridTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    class GameWorldScreenGridTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            Immovable floor = new Immovable();
            floor.SetScale(50, 1, 50);
            floor.SetPosition(0, -0.5f, 0);
            floor.IsShadowCaster = true;
            AddGameObject(floor);

            Immovable wallLeft = new Immovable();
            wallLeft.SetScale(1, 5, 50);
            wallLeft.SetPosition(-24.5f, 2.5f, 0);
            wallLeft.IsShadowCaster = true;
            AddGameObject(wallLeft);

            Immovable wallRight = new Immovable();
            wallRight.SetScale(1, 5, 50);
            wallRight.SetPosition(24.5f, 2.5f, 0);
            wallRight.IsShadowCaster = true;
            AddGameObject(wallRight);

            Immovable wallBack = new Immovable();
            wallBack.SetScale(48, 5, 1);
            wallBack.SetPosition(0, 2.5f, -24.5f);
            wallBack.IsShadowCaster = true;
            AddGameObject(wallBack);

            Immovable wallFront = new Immovable();
            wallFront.SetScale(48, 5, 1);
            wallFront.SetPosition(0, 2.5f, 24.5f);
            wallFront.IsShadowCaster = true;
            AddGameObject(wallFront);


            SetColorAmbient(0.25f, 0.25f, 0.25f);
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(25, 25, 25);
            sun.SetColor(1, 1, 1, 2.5f);
            sun.SetFOV(75);
            AddLightObject(sun);

            PlayerFreeFloat p = new PlayerFreeFloat();
            p.SetPosition(0, 20, 40);
            AddGameObject(p);
            MouseCursorGrab();

            LightObject dLight1 = new LightObject(LightType.Directional, ShadowQuality.NoShadow);
            dLight1.SetColor(1, 0, 0, 7.5f);
            dLight1.SetPosition(-20, 1, -20);
            dLight1.SetTarget(-10, 0, -15);
            dLight1.SetNearFar(1, 20);
            AddLightObject(dLight1);

            LightObject dLight2 = new LightObject(LightType.Directional, ShadowQuality.NoShadow);
            dLight2.SetColor(0, 1, 1, 7.5f);
            dLight2.SetPosition(-20, 1, 20);
            dLight2.SetTarget(-10, 0, 10);
            dLight2.SetNearFar(1, 20);
            AddLightObject(dLight2);

            LightObject pLight1 = new LightObject(LightType.Point, ShadowQuality.NoShadow);
            pLight1.SetColor(1, 0, 1, 7.5f);
            pLight1.SetPosition(10, 1, 0);
            pLight1.SetNearFar(1, 20);
            AddLightObject(pLight1);
        }
    }
}
