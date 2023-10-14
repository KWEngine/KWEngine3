using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Models.WorldKWQuadTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldKWQuadTest : World
    {
        
        public override void Act()
        {
           
        }

        public override void Prepare()
        {
            SetCameraFOV(10);
            SetCameraPosition(0, 100, 0);
            SetCameraTarget(0, 0, 0);

            PlayerQuad p = new PlayerQuad();
            p.SetModel("KWQuad");
            p.SetTexture("./Textures/spritesheet.png");
            p.SetTextureRepeat(1 / 10f, 1 / 3f);
            p.SetScale(5);
            p.HasTransparencyTexture = true;
            AddGameObject(p);

        }
    }
}
