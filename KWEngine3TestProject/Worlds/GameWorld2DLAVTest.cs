using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.World2DLAVTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;


namespace KWEngine3TestProject.Worlds
{
    internal class GameWorld2DLAVTest : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 0, 100);
            SetCameraTarget(0, 0, 0);
            SetCameraFOV(10);

            Player p = new Player();
            p.SetModel("KWQuad");
            p.SetPosition(-5, -2, 0);
            p.SetTexture("./Textures/char2dright.png");
            AddGameObject(p);

            Target t = new Target();
            t.Name = "T";
            t.SetModel("KWQuad");
            t.SetPosition(5, 3, 0);
            t.SetTexture("./Textures/Trauersmiley.png");
            AddGameObject(t);
        }
    }
}
