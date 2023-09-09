using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldHUDTest : World
    {
        public override void Act()
        {
            HUDObject start = GetHUDObjectByName("Start");
            if (start != null)
            {
                if (start.IsMouseCursorOnMe())
                {
                    start.SetColorGlowIntensity(1);
                    if (Mouse[MouseButton.Left])
                    {
                        
                    }
                }
                else
                {
                    start.SetColorGlowIntensity(0);
                }
            }

            HUDObject quit = GetHUDObjectByName("Quit");
            if (quit != null)
            {
                if (quit.IsMouseCursorOnMe())
                {
                    quit.SetColorGlowIntensity(1);
                    if (Mouse[MouseButton.Left])
                    {
                        Window.Close();
                    }
                }
                else
                {
                    quit.SetColorGlowIntensity(0);
                }
            }

            HUDObject image = GetHUDObjectByName("Image");
            if (image != null)
            {
                if (image.IsMouseCursorOnMe())
                {
                    image.SetColorGlowIntensity(1);
                    if (Mouse[MouseButton.Left])
                    {
                        Window.Close();
                    }
                }
                else
                {
                    image.SetColorGlowIntensity(0);
                }
            }
        }

        public override void Prepare()
        {
            HUDObject h1 = new HUDObject(HUDObjectType.Text, 0, 0);
            h1.Name = "Start";
            h1.SetText("Start Game");
            h1.SetColorGlow(1, 0, 0);
            h1.SetPosition(Window.Width / 2 - "Start Game".Length / 2 * h1.CharacterSpreadFactor, Window.Height / 2.5f);
            AddHUDObject(h1);

            HUDObject h2 = new HUDObject(HUDObjectType.Text, 0, 0);
            h2.Name = "Quit";
            h2.SetText("Quit");
            h2.SetColorGlow(1, 0, 0);
            h2.SetPosition(Window.Width / 2 - "Quit".Length / 2 * h2.CharacterSpreadFactor, Window.Height / 1.5f);
            AddHUDObject(h2);

            HUDObject h3 = new HUDObject(HUDObjectType.Image, Window.Width / 2, Window.Height / 2);
            h3.Name = "Image";
            h3.SetColorGlow(1, 0, 0);
            h3.SetTexture(@".\Textures\Leaves_01_512_alpha.png");
            AddHUDObject(h3);
        }
    }
}
