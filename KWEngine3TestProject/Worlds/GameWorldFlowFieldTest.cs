﻿using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldFlowFieldTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.ComponentModel.Design.Serialization;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldFlowFieldTest : World
    {
        public override void Act()
        {
            Vector3 cursorPosWorld = HelperIntersection.GetMouseIntersectionPointOnPlane(Plane.XZ, 0);
            FlowField f = GetFlowField();
            if (f != null)
            {
                if (Keyboard.IsKeyDown(Keys.Left))
                {
                    f.SetPosition(f.Center.X - 0.005f, f.Center.Z - 0.005f);
                }
                else if(Keyboard.IsKeyDown(Keys.Right))
                {
                    f.SetPosition(f.Center.X + 0.005f, f.Center.Z + 0.005f);
                }

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

                
                if (Mouse.IsButtonPressed(MouseButton.Left))
                {

                    f.SetTarget(cursorPosWorld);
                }
                
            }
            
            if (Keyboard.IsKeyPressed(Keys.F2))
            {
                Window.SetWorld(new GameWorldFlowFieldTest2());
            }
            
        }

        public override void Prepare()
        {
            SetCameraPosition(0, 15, 0);
            SetCameraTarget(0, 0, 0);
            SetColorAmbient(1, 1, 1);

            Impassable i1 = new Impassable();
            i1.SetScale(4, 1, 2);
            i1.SetColor(1, 1, 1);
            i1.FlowFieldCost = 255;
            i1.IsCollisionObject = true;
            AddGameObject(i1);

            
            Impassable i2 = new Impassable();
            i2.SetScale(4, 1, 2);
            i2.SetColor(1, 1, 1);
            i2.SetPosition(0, 0, -3);
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
            floor.SetColor(0.25f, 0.5f, 0);
            AddGameObject(floor);

            FlowField f1 = new FlowField(-7.5f, 0, 0, 20, 20, 0.25f, 1, FlowFieldMode.Simple, typeof(Impassable));
            f1.Name = "F1";
            f1.IsVisible = true;
            AddFlowField(f1);

            FlowField f2 = new FlowField(7.5f, 0, 0, 20, 20, 0.25f, 1, FlowFieldMode.Simple, typeof(Impassable));
            f2.Name = "F2";
            f2.IsVisible = true;
            AddFlowField(f2);
        }
    }
}
