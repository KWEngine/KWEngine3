﻿using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldPlaneCollisionTest;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldPlaneCollisionTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 10);
            SetColorAmbient(0.5f, 0.5f, 0.5f);

            KWEngine.LoadModel("Beach_Render", "./Models/GLTFTest/WorldADV03_Beach_Map.obj");
            KWEngine.LoadModel("Beach_Planes", "./Models/GLTFTest/WorldADV03_Beach_Floors.glb");
            KWEngine.LoadModel("Beach_Hitboxes", "./Models/GLTFTest/WorldADV03_Beach_Hitboxes.glb");
            KWEngine.LoadModel("FHBTEST", "./Models/GLTFTest/fullhitboxtest.glb");
            /*
            MapRender r = new MapRender();
            r.SetModel("Beach_Render");
            AddRenderObject(r);

            Immovable x = new Immovable();
            x.SetModel("Beach_Planes");
            x.SetCollisionType(ColliderType.RayCollider);
            //x.SetOpacity(0);
            AddGameObject(x);

            Immovable y = new Immovable();
            y.SetModel("Beach_Hitboxes");
            y.SetCollisionType(ColliderType.ConvexHull);
            //y.SetOpacity(0);
            AddGameObject(y);
            */

            Immovable i01 = new Immovable();
            i01.Name = "Floor";
            i01.SetScale(10, 1, 10);
            i01.SetPosition(0, -0.5f, 0f);
            i01.SetColliderType(ColliderType.ConvexHull);
            i01.IsShadowCaster = true;
            AddGameObject(i01);
            
            Immovable i02 = new Immovable();
            i02.Name = "PlatformPurple";
            i02.SetScale(1, 0.25f, 1);
            i02.SetPosition(3, 0.375f, -3f);
            i02.SetColor(0, 1, 1);
            i02.SetColliderType(ColliderType.ConvexHull);
            i02.IsShadowCaster = true;
            AddGameObject(i02);

            Immovable i03 = new Immovable();
            i03.SetModel("FHBTEST");
            i03.Name = "PlatformYellowExtended";
            i03.SetScale(1, 0.25f, 1);
            i03.SetPosition(-3, 0.375f, 3f);
            i03.SetColor(1, 1, 0);
            i03.SetColliderType(ColliderType.ConvexHull);
            i03.IsShadowCaster = true;
            AddGameObject(i03);

            Player p = new Player();
            p.SetModel("KWCube");
            //p.SetPosition(3, 0.25f, -4);
            //p.SetPosition(0, 0.25f, 0);
            p.SetPosition(-3, 0.75f, 3);
            p.SetColor(1, 0, 1);
            p.SetScale(0.5f);
            p.SetColliderType(ColliderType.ConvexHull);
            p.IsShadowCaster = true;
            AddGameObject(p);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(25, 25, 0);
            sun.SetColor(1, 1, 1, 2);
            sun.SetFOV(20);
            sun.SetNearFar(10, 100);
            AddLightObject(sun);

        }
    }
}