using OpenTK;
using OpenTK.Mathematics;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldGLTFNodesTest;
using System;
using System.Collections.Generic;
using KWEngine3.Helper;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldGLTFNodesTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("ModelImport", "./Models/GLTFTest/cube_notangents.glb");
            SetCameraPosition(10, 10, 10);
            SetColorAmbient(0.25f, 0.25f, 0.25f);
            
            ModelTestClass g = new ModelTestClass();
            g.SetModel("ModelImport");
            g.IsShadowCaster = true;
            //g.HasTransparencyTexture = false;
            //g.DisableBackfaceCulling = false;

            int instanceCount = 0;
            g.SetAdditionalInstanceCount(instanceCount);
            
            for(int i = 1; i < instanceCount + 1; i++)
            {
                float x = HelperRandom.GetRandomNumber(-24, 24);
                float z = HelperRandom.GetRandomNumber(-24, 24);
                g.SetPositionRotationScaleForInstance(i, new Vector3(x, 0, z), Quaternion.Identity, Vector3.One);
            }
            
            AddRenderObject(g);
            
            
            //Floor f = new Floor();
            //f.SetPosition(0, -0.5f, 0);
            //f.SetScale(50, 1, 50);
            //f.IsShadowCaster = true;
            //AddGameObject(f);
            
            LightObject sun = new LightObject(LightType.Sun, ShadowQuality.High);
            sun.Name = "Sonne";
            sun.SetPosition(-50, 50, 50);
            sun.SetColor(1, 1, 1, 2);
            sun.SetNearFar(10, 200);
            sun.SetFOV(100);
            AddLightObject(sun);
            
        }
    }
}
