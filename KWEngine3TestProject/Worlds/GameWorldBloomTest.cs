using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldBloomTest : World
    {
        public override void Act()
        {
           
        }

        public override void Prepare()
        {
            SetCameraFOV(90);
            SetCameraPosition(0, 0, 25);

            Immovable i01 = new Immovable();
            i01.SetPosition(-6, 0, 0);
            i01.SetScale(0.25f, 0.1f, 0.1f);
            i01.SetColor(1, 0.5f, 0.25f);
            i01.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i01);

            Immovable i02 = new Immovable();
            i02.SetPosition(-4, 0, 0);
            i02.SetScale(0.5f, 0.1f, 0.1f);
            i02.SetColor(1, 0.5f, 0.25f);
            i02.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i02);

            Immovable i03 = new Immovable();
            i03.SetPosition(-2, 0, 0);
            i03.SetScale(0.1f, 0.5f, 0.1f);
            i03.SetColor(1, 0.5f, 0.25f);
            i03.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i03);

            Immovable i04 = new Immovable();
            i04.SetPosition(0, 0, 0);
            i04.SetScale(0.5f, 0.5f, 0.1f);
            i04.SetColor(1, 0.5f, 0.25f);
            i04.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i04);
        }
    }
}
