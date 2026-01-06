using KWEngine3;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldFX;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFX : World
    {
        public override void Act()
        {
            if(Keyboard.IsKeyDown(Keys.Left))
            {
                SetCameraPosition(CameraPosition.X - 0.05f, CameraPosition.Y, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X - 0.05f, CameraTarget.Y, CameraTarget.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.Right))
            {
                SetCameraPosition(CameraPosition.X + 0.05f, CameraPosition.Y, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X + 0.05f, CameraTarget.Y, CameraTarget.Z);
            }
        }
        public override void Prepare()
        {
            SetCameraPosition(0, 2, 5);
            SetColorAmbient(1f, 1f, 1f);

            KWEngine.LoadModel("Megan", "./Models/GLTFTest/megan.gltf");
            KWEngine.LoadModel("Aura", "./Models/KWFXCircleWide.glb");
            KWEngine.LoadModel("Shield", "./Models/KWFXShield.glb");

            Floor f = new Floor();
            f.SetModel("KWCube");
            f.SetTexture("./Textures/grass_albedo.png", TextureType.Albedo);
            f.SetScale(20f, 1f, 20f);
            f.SetPosition(0f, -0.5f, 0f);
            AddGameObject(f);

            Aura a = new Aura();
            a.SetModel("Shield");
            //a.SetPosition(0f, 0.0125f, 0f);
            a.SetPosition(0f, 0.5f, 0f);
            a.SetScale(2f);
            a.SetTexture("./Textures/rune.png", TextureType.Albedo, 0);
            a.SetTexture("./Textures/rune.png", TextureType.Albedo, 1);
            a.SetColorEmissive(1, 0.5f, 0, 1);
            a.SetTextureRepeat(15, 1, 0);
            a.SetTextureRepeat(15, 1, 1);
            a.DisableBackfaceCulling = true;
            //a.SetOpacity(1, 1);
            a.HasTransparencyTexture = true;
            AddGameObject(a);

            Player player = new Player();
            player.SetModel("Megan");
            player.SetAnimationID(0);
            player.SetPosition(0f, 0f, 0f);
            player.SetAura(a);
            AddGameObject(player);

            /*
            TextObject to = new TextObject("Test");
            to.SetPosition(0, 1, 2.5f);
            to.SetTextAlignment(TextAlignMode.Right);
            AddTextObject(to);

            TextObject to2 = new TextObject("Test Test");
            to2.SetPosition(0, 3, 2.5f);
            to2.SetTextAlignment(TextAlignMode.Center);
            AddTextObject(to2);
            */
        }
    }
}
