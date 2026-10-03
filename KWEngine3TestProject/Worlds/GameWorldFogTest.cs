using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFogTest : World
    {

        public override void Act()
        {

        }

        public override void Prepare()
        {


            Player p1 = new Player();
            p1.Name = "Player #1";
            p1.IsCollisionObject = true;
            p1.SkipRender = true;
            p1.SetModel("KWCube");
            p1.SetHitboxScale(1, 1, 1);
            p1.SetColor(1, 1, 0);
            p1.SetPosition(0, 0.5f, 4);
            p1.SetRotation(0, 180, 0);
            AddGameObject(p1);

            Immovable floor = new Immovable();
            floor.SetScale(10, 1, 10);
            floor.SetPosition(0, -0.5f, 0);
            floor.SetTexture("./Textures/grass_albedo.png");
            floor.SetTextureRepeat(5, 5);
            AddGameObject(floor);

            Immovable quad = new Immovable();
            quad.SetModel("KWQuad");
            quad.HasTransparencyTexture = true;
            quad.SetTexture("./Textures/Trauersmiley.png");
            quad.SetPosition(0, 2, -2.5f);
            AddGameObject(quad);

            SetCameraFOV(90);
            MouseCursorGrab();
            SetCameraToFirstPersonGameObject(p1, 0.5f);

            SetFogDensity(0.2f);
            SetFogHeight(0.1f, 0.95f);
            SetFogColor(1, 0, 1);
        }
    }
}
