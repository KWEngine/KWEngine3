using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldFirstPersonView;
using OpenTK.Mathematics;


namespace KWEngine3TestProject.Worlds
{
    public class GameWorldFirstPersonView : World
    {
        public override void Act()
        {
          
        }

        public override void Prepare()
        {
            SetCameraFOV(90);
            KWEngine.LoadModel("fpsArms", @".\Models\FirstPersonView\fps_arms.fbx");
            KWEngine.LoadModel("fpsGun", @"./Models/PlatformerPack/Gun.gltf");
            /*
            List<string> bones = KWEngine.GetModelBoneNames("fpsArms");
            foreach(string bone in bones)
            {
                KWEngine.LogWriteLine(bone); // hand.R
            }
            */
            Immovable f01 = new Immovable();
            f01.Name = "Floor";
            f01.SetModel("KWCube");
            f01.SetTexture(@"./textures/tiles_albedo.jpg", TextureType.Albedo);       // regular texture file
            f01.SetTexture(@"./textures/tiles_normal.jpg", TextureType.Normal);       // (optional) normal map for custom light reflections
            f01.SetTexture(@"./textures/tiles_roughness.png", TextureType.Roughness); // (optional) roughness map for specular highlights
            f01.SetTextureRepeat(3, 3);                                               // how many times the texture is tiled across the object?
            f01.SetScale(15, 0.2f, 15);
            f01.SetPosition(0, -0.1f, 0);
            f01.IsCollisionObject = true;
            f01.IsShadowCaster = true;
            AddGameObject(f01);

            Wall w1 = new Wall();
            w1.SetScale(5, 2, 1);
            w1.SetPosition(0, 1, 0);
            w1.SetTexture("./Textures/Brick_01_512.png");
            w1.SetTextureRepeat(5, 2);
            w1.IsCollisionObject = true;
            w1.IsShadowCaster = true;
            AddGameObject(w1);

            PlayerFirstPerson p01 = new PlayerFirstPerson();
            p01.SetModel("KWCube");
            p01.Name = "Player #1";
            p01.SetPosition(-2.5f, 1.0f, -1.25f);
            p01.SetScale(1.0f, 2.0f, 1.0f);
            p01.IsCollisionObject = true;
            p01.UpdateLast = true;
            AddGameObject(p01);

            Attachment att01 = new Attachment();
            att01.Name = "Attachment";
            att01.SetColor(0, 0, 1);
            att01.SetScale(0.25f);
            att01.SetPosition(0, 0.125f, 3);
            att01.SetColorEmissive(0, 0, 1, 1);
            AddGameObject(att01);

            MouseCursorGrab();
            SetCameraToFirstPersonGameObject(p01, 0.5f);
            ViewSpaceWeapon vsw = new ViewSpaceWeapon();
            vsw.SetModel("fpsGun");
            vsw.SetOffset(0.25f, -0.25f, 0.75f);
            vsw.SetRotation(0, 15, 0);
            vsw.SetScale(0.25f);
            vsw.IsShadowCaster = true;
            vsw.DepthTestingEnabled = false;
            SetViewSpaceGameObject(vsw);

            LightObject lo = new LightObject(LightType.Point, ShadowQuality.High);
            lo.SetPosition(0, 3, -2);
            lo.SetColor(1, 1, 0, 3);
            AddLightObject(lo);
        }
    }
}
