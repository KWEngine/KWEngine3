using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldGLTFNodesTest;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldGLTFNodesTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Grass1", "./Models/GLTFTest/grass/grass1.gltf");
            SetCameraPosition(10, 10, 10);
            SetColorAmbient(0.25f, 0.25f, 0.25f);

            Grass g = new Grass();
            g.SetModel("Grass1");
            g.IsShadowCaster = true;
            g.HasTransparencyTexture = true;
            g.DisableBackfaceCulling = true;
            AddRenderObject(g);

            Floor f = new Floor();
            f.SetPosition(0, -0.5f, 0);
            f.SetScale(50, 1, 50);
            f.IsShadowCaster = true;
            AddGameObject(f);

            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.SetPosition(-50, 50, 50);
            sun.SetColor(1, 1, 1, 2);
            sun.SetNearFar(10, 200);
            sun.SetFOV(100);
            AddLightObject(sun);
        }
    }
}
