using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldThirdPersonView;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    public class GameWorldThirdPersonView : World
    {
        private bool _paused = false;

        public bool IsPaused()
        {
            return _paused;
        }
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.P))
            {
                Console.WriteLine("p: " + WorldTime);
                _paused = !_paused;
                if(_paused)
                {
                    MouseCursorReset();
                }
                else
                {
                    MouseCursorGrab();
                }
            }
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("UBot", @".\Models\ThirdPersonView\ubot.fbx");

           
            SetBackgroundBrightnessMultiplier(4);
            SetBackgroundSkybox(@".\Textures\skybox.dds");

            
            Floor f01 = new Floor();
            f01.Name = "Floor";
            f01.SetModel("KWCube");
            f01.SetScale(15, 1f, 15);
            f01.SetPosition(0, -0.5f, 0);
            f01.IsShadowCaster = true;                                                // does the object cast and receive shadows? (default: false)
            f01.IsCollisionObject = true;
            AddGameObject(f01);
            

            PlayerThirdPerson p01 = new PlayerThirdPerson();
            p01.SetModel("UBot");
            p01.SetPosition(0, 10, 0);
            p01.SetScale(1);
            p01.IsShadowCaster = true;
            p01.IsCollisionObject = true;
            p01.SetRotation(0, 180, 0);
            p01.SetHitboxToCapsule();
            AddGameObject(p01);

            CollisionChecker cc = new CollisionChecker();
            cc.SetModel("KWSphere");
            cc.IsCollisionObject = true;
            cc.SkipRender = true;
            cc.SetScale(0.5f);
            AddGameObject(cc);
            p01.SetCollisionChecker(cc);
            
            

            Immovable box01 = new Immovable();
            box01.SetScale(2);
            box01.SetPosition(4, 10, -4);
            box01.IsCollisionObject = true;
            box01.IsShadowCaster = true;
            AddGameObject(box01);
            /*
            HUDObject crosshair = new HUDObject(HUDObjectType.Image, Window.Width / 2, Window.Height / 2);
            crosshair.Name = "Crosshair";
            crosshair.SetTexture(@".\textures\crosshair.dds");
            AddHUDObject(crosshair);
            */

            
            LightObject sun = new LightObjectSun(ShadowQuality.Medium, SunShadowType.Default);
            sun.Name = "Sun";
            sun.SetFOV(32);
            sun.SetNearFar(1, 200);
            sun.SetPosition(50, 15, 20);
            sun.SetColor(1, 0.9f, 0.8f, 2.5f);
            AddLightObject(sun);
            
            
            SetColorAmbient(0.25f, 0.25f, 0.25f);
            KWEngine.BuildTerrainModel("t", "./Textures/heightmap512.png", 25);
            TerrainObject t = new TerrainObject("t");
            t.SetTexture("./Textures/sand_diffuse.dds");
            t.SetTexture("./Textures/sand_normal.dds", TextureType.Normal);
            t.SetTextureRepeat(1, 1);
            t.IsCollisionObject = true;
            //t.IsShadowCaster = true;
            t.SetColor(1, 0, 0);
            t.Name = "t";
            t.TessellationThreshold = TerrainThresholdValue.T64;
            AddTerrainObject(t);

            //KWEngine.DebugMode = DebugMode.TerrainCollisionModel;

            MouseCursorGrab();

            SetCameraPosition(0, 150, 0);
            SetCameraTarget(0, 0, 0);
        }
    }
}
