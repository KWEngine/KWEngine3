using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldFlowFieldTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFlowFieldTest : World
    {
        private Vector3 _lastTarget = Vector3.Zero;
        public override void Act()
        {
            Vector3 cursorPosWorld = HelperIntersection.GetMouseIntersectionPointOnPlane(Plane.XZ, 0);
            /*
            int r = HelperRandom.GetRandomNumber(1, 60);
            if(r == 1)
            {
                Impassable i = new Impassable();
                i.SetScale(4, 1, 2);
                i.SetColor(1, 1, 1);
                i.SetPosition(HelperRandom.GetRandomNumber(-10f, 10f), 0, HelperRandom.GetRandomNumber(-10f, 10f));
                i.FlowFieldCost = 255;
                i.IsCollisionObject = true;
                AddGameObject(i);

                for (int j = 0; j < GetFlowFieldCount(); j++)
                {
                    GetFlowField(j).Update();
                }
            }
            if(r == 2)
            {
                List<Impassable> i = GetGameObjectsByType<Impassable>();
                if(i.Count > 0)
                {
                    RemoveGameObject(i[0]);
                    for (int j = 0; j < GetFlowFieldCount(); j++)
                    {
                        GetFlowField(j).Update();
                    }
                }
            }
            
            if(r == 3)
            {

            }
            */


            FlowField f = GetFlowField();
            if (f != null)
            {
                
                if (Keyboard.IsKeyDown(Keys.Left))
                {
                    f.SetPosition(f.Center.X - 0.005f, f.Center.Z - 0.005f);
                    //if (_lastTarget != Vector3.Zero) f.SetTarget(_lastTarget, true);
                }
                else if(Keyboard.IsKeyDown(Keys.Right))
                {
                    f.SetPosition(f.Center.X + 0.005f, f.Center.Z + 0.005f);
                    //if(_lastTarget != Vector3.Zero) f.SetTarget(_lastTarget, true);
                }

                /*
                FlowFieldCell ffc = f.GetCellForWorldPosition(cursorPosWorld);
                //Console.WriteLine(ffc.IndexX + "|" + ffc.IndexZ);
                if (ffc != null)
                {
                    FlowFieldCell ffc2 = ffc.GetNeighbourCellAtOffset(0, 0);
                    if(ffc2 != null)
                    {
                        Console.WriteLine(ffc2.IndexX + "|" + ffc2.IndexZ + ": " + ffc2.Cost);
                    }
                    else
                    {
                        Console.WriteLine("offset invalid");
                    }
                }
                else
                {
                    Console.WriteLine("Cursor not inside flow field");
                }
                */

                if (Mouse.IsButtonPressed(MouseButton.Left))
                {
                    _lastTarget = cursorPosWorld;
                    f.SetTarget(_lastTarget);
                }
                
            }
            
            
            if (Keyboard.IsKeyPressed(Keys.F2))
            {
                Window.SetWorld(new GameWorldFlowFieldTest2());
            }
            
        }

        public override void Prepare()
        {
            KWEngine.LoadModel("Knezi", "./Models/knezi1.glb");

            SetCameraPosition(0, 15, 0);
            SetCameraTarget(0, 0, 0);
            SetColorAmbient(1, 1, 1);

            Impassable i1 = new Impassable();
            i1.SetModel("Knezi");
//            i1.SetScale(4, 1, 2);
//            i1.SetColor(1, 1, 1);
            i1.FlowFieldCost = 255;
            i1.Name = "Map";
            i1.IsCollisionObject = true;
            AddGameObject(i1);

            /*
            Impassable i2 = new Impassable();
            i2.SetScale(4, 1, 2);
            i2.SetColor(1, 1, 1);
            i2.SetPosition(0, 0, -3);
            i2.FlowFieldCost = 255;
            i2.IsCollisionObject = true;
            AddGameObject(i2);
*/
            

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
            floor.SetColor(0.25f, 0.5f, 0);
            AddGameObject(floor);

            FlowField f1 = new FlowField(-5f, 2, 0, 20, 20, 0.25f, 5, FlowFieldMode.Box, typeof(Impassable));
            f1.Name = "F1";
            f1.IsVisible = true;
            AddFlowField(f1);

            /*FlowField f2 = new FlowField(5f, 0, 0, 20, 20, 0.25f, 1, FlowFieldMode.Simple, typeof(Impassable));
            f2.Name = "F2";
            f2.IsVisible = true;
            AddFlowField(f2);*/
        }
    }
}
