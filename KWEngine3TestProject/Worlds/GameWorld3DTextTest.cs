﻿using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldReuseTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorld3DTextTest : World
    {
        private TextObject t1;
        public override void Act()
        {
            /*
            TextObject t = GetTextObjectByName("Test");
            if(t != null)
            {
                if (Keyboard[Keys.Q])
                    t.SetPosition(t.Position.X - 0.01f, t.Position.Y, t.Position.Z);
                if (Keyboard[Keys.E])
                    t.SetPosition(t.Position.X + 0.01f, t.Position.Y, t.Position.Z);

                KWEngine.LogWriteLine(t.Position);
                KWEngine.LogWriteLine(t.IsInsideScreenSpace);
                KWEngine.LogWriteLine("-------");
            }
            */

            if(Keyboard.IsKeyDown(Keys.Left))
            {
                SetCameraPosition(CameraPosition.X - 0.05f, CameraPosition.Y, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X - 0.05f, CameraTarget.Y, CameraTarget.Z);
            }
            else if(Keyboard.IsKeyDown(Keys.Right))
            {
                SetCameraPosition(CameraPosition.X + 0.05f, CameraPosition.Y, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X + 0.05f, CameraTarget.Y, CameraTarget.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.Up))
            {
                SetCameraPosition(CameraPosition.X, CameraPosition.Y + 0.05f, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X, CameraTarget.Y + 0.05f, CameraTarget.Z);
            }
            else if (Keyboard.IsKeyDown(Keys.Down))
            {
                SetCameraPosition(CameraPosition.X, CameraPosition.Y - 0.05f, CameraPosition.Z);
                SetCameraTarget(CameraTarget.X, CameraTarget.Y - 0.05f, CameraTarget.Z);
            }

            //Console.WriteLine(t1.IsInsideScreenSpace);
            
        }

        public override void Prepare()
        {
            SetCameraFOV(90);
            SetCameraPosition(5.0f, 5.0f, 2.5f);
            /*
            Immovable i01 = new Immovable();
            i01.Name = "Floor";
            i01.IsCollisionObject = true;
            i01.SetPosition(0, -4, 0);
            i01.SetScale(5, 2, 2);
            i01.SetRotation(0, 0, 0);
            i01.SetColor(1, 0, 1);
            AddGameObject(i01);
            */
            Player p1 = new Player();
            p1.Name = "Player #1";
            p1.IsCollisionObject = true;
            p1.SetModel("KWCube");
            p1.SetHitboxScale(1, 1, 1);
            p1.SetColor(1, 1, 0);
            p1.SetPosition(0, 0, -1);
            AddGameObject(p1);
            
            t1 = new TextObject("The QUICK brown fox jumped over the lazy dog.");
            t1.Name = "Test";
            //t1.SetColorEmissive(1, 0, 1, 1.5f);
            t1.SetFont(FontFace.OpenSans);
            t1.SetScale(1.0f);
            t1.SetCharacterDistanceFactor(1f);
            AddTextObject(t1);
            /*
            TextObject t2 = new TextObject("Dies iost ein Test");
            t2.Name = "Test2";
            t2.SetColorEmissive(1, 1, 1, 0.5f);
            t2.SetFont(FontFace.NovaMono);
            t2.SetScale(0.5f);
            t2.SetPosition(5, 0, -1);
            t2.SetCharacterDistanceFactor(0.75f);
            AddTextObject(t2);
            */
        }
    }
}
