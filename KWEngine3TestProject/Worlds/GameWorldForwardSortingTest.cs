using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using KWEngine3TestProject.Classes.WorldReuseTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldForwardSortingTest : World
    {
        private TextObject t1;
        private TextObject t2;
        private RenderObjectDefault r1;
        private Immovable g1;

        public override void Act()
        {
            if (Keyboard.IsKeyPressed(Keys.F1))
            {
                PlaceObjects(1);
            }
            else if (Keyboard.IsKeyPressed(Keys.F2))
            {
                PlaceObjects(2);
            }
            else if (Keyboard.IsKeyPressed(Keys.F3))
            {
                PlaceObjects(3);
            }
            else if (Keyboard.IsKeyPressed(Keys.F4))
            {
                PlaceObjects(4);
            }
        }

        private void PlaceObjects(int variant)
        {
            if (variant > 4)
                variant = 1;

            KWEngine.LogWriteLine("Variante: " + variant);
            if (variant == 1)
            {
                t1.SetPosition(0, 0, -5);
                t2.SetPosition(0, 0, -4);
                r1.SetPosition(0, 0, -3);
                g1.SetPosition(0, 0, -2);
            }
            else if (variant == 2)
            {
                r1.SetPosition(0, 0, -5);
                t1.SetPosition(0, 0, -4);
                g1.SetPosition(0, 0, -3);
                t2.SetPosition(0, 0, -2);
            }
            else if (variant == 3)
            {
                t1.SetPosition(0, 0, -5);
                r1.SetPosition(0, 0, -4);
                t2.SetPosition(0, 0, -3);
                g1.SetPosition(0, 0, -2);
            }
            else if (variant == 4)
            {
                g1.SetPosition(0, 0, -5);
                r1.SetPosition(0, 0, -4);
                t1.SetPosition(0, 0, -3);
                t2.SetPosition(0, 0, -2);

            }
        }

        public override void Prepare()
        {
            KWEngine.DebugOverlayMode = DebugOverlayMode.MembersAndConsole;

            SetCameraFOV(20);
            SetCameraPosition(-10f, 10f, 25f);
            SetCameraTarget(-2.5f, 0f, 0f);

            t1 = new TextObject("T1 T1 T1 T1 T1");
            t1.Name = "T1";
            t1.SetFont("Anonymous");
            t1.SetScale(1.0f);
            t1.SetCharacterDistanceFactor(1.0f);
            AddTextObject(t1);

            t2 = new TextObject("T2 T2 T2 T2 T2");
            t2.Name = "T2";
            t2.SetFont("Anonymous");
            t2.SetScale(1.0f);
            t2.SetCharacterDistanceFactor(1.0f);
            AddTextObject(t2);

            r1 = new RenderObjectDefault();
            r1.SetModel("KWQuad");
            r1.Name = "R1";
            r1.SetTexture("./Textures/bush01_albedo.png");
            r1.HasTransparencyTexture = true;
            r1.SetScale(4f);
            AddRenderObject(r1);

            g1 = new Immovable();
            g1.SetModel("KWQuad");
            g1.Name = "G1";
            g1.SetTexture("./Textures/fx_boom.png");
            g1.HasTransparencyTexture = true;
            g1.SetScale(4f);
            AddGameObject(g1);

            PlaceObjects(1);
        }
    }
}
