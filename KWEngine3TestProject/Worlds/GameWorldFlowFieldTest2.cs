using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldFlowFieldTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFlowFieldTest2 : World
    {
        public override void Act()
        {
            
            if(Mouse.IsButtonPressed(MouseButton.Left))
            {
                Vector3 cursorPosWorld = HelperIntersection.GetMouseIntersectionPointOnPlane(Plane.XZ, 0);
                FlowField f = GetFlowField();
                if(f != null)
                    f.SetTarget(cursorPosWorld);
            }
            
            if(Keyboard.IsKeyPressed(Keys.F1))
            {
                Window.SetWorld(new GameWorldFlowFieldTest());
            }
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 15, 0);
            SetCameraTarget(0, 0, 0);
            SetColorAmbient(1, 1, 1);

            Impassable test = new Impassable();
            test.SetScale(4, 1, 2);
            test.SetColor(1, 1, 1);
            test.SetPosition(0, 0.5f, -2);
            test.FlowFieldCost = 255;
            test.IsCollisionObject = true;
            AddGameObject(test);

            Impassable i2 = new Impassable();
            i2.SetScale(4, 1, 2);
            i2.SetColor(1, 1, 1);
            i2.SetPosition(0, 0, 3);
            i2.FlowFieldCost = 255;
            i2.IsCollisionObject = true;
            AddGameObject(i2);

            Player p = new Player();
            p.SetScale(1);
            p.SetPosition(-4.5f, 0.5f, 3.5f);
            p.SetColor(1, 1, 0);
            p.IsCollisionObject = true;
            AddGameObject(p);

            Enemy e = new Enemy();
            e.SetModel("KWSphere");
            e.SetPosition(4.5f, 0.5f, 3.5f);
            e.SetColor(1, 0, 1);
            e.IsCollisionObject = true;
            AddGameObject(e);

            Immovable floor = new Immovable();
            floor.SetScale(20, 1, 16);
            floor.SetPosition(0, -0.5f, 0);
            floor.SetColor(0, 1, 1);
            AddGameObject(floor);

            FlowField f = new FlowField(10, 8, 0.5f, 1, FlowFieldMode.Simple, typeof(Impassable));
            f.IsVisible = true;
            f.AllowPartialNesting = false;
            f.SetAllowedDirections(FlowFieldDirections.CardinalDirections);
            AddFlowField(f);
        }
    }
}
