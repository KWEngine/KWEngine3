using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using KWEngine3TestProject.Worlds;


namespace KWEngine3TestProject.Classes.WorldFirstPersonView
{
    public class PlayerFirstPerson : GameObject
    {
        //private float _lastKeyPress = float.MinValue;
        private float bobZ = 0f;
        private float bobY = 0f;
        private float bobX = 0f;
        private float bobTime = 0f;

        public override void Act()
        {
            
            /*
            if(HelperRandom.GetRandomNumber(0, 256) == 0)
            {
                CurrentWorld.StartCameraShake(3, 3, 3);
            }
            */

            if(Keyboard.IsKeyPressed(Keys.F5))
            {
                if (Window.VSync == OpenTK.Windowing.Common.VSyncMode.On)
                {
                    Window.VSync = OpenTK.Windowing.Common.VSyncMode.Off;
                    Console.WriteLine("VSYNC OFF");
                }
                else
                {
                    Window.VSync = OpenTK.Windowing.Common.VSyncMode.On;
                    Console.WriteLine("VSYNC ON");
                }
            }

            if(CurrentWorld is GameWorldFirstPersonView && (CurrentWorld as GameWorldFirstPersonView).IsPaused())
            {
                return;
            }


            ViewSpaceGameObject vsg = CurrentWorld.GetViewSpaceGameObject();
            if (vsg != null && (Keyboard.IsKeyPressed(Keys.D1) || Keyboard.IsKeyPressed(Keys.D2)))
            {
                //if (WorldTime - _lastKeyPress > 0.125f)
                //{
                    Attachment att = CurrentWorld.GetGameObjectByName<Attachment>("Attachment");
                    if (att != null)
                    {
                        if (Keyboard.IsKeyDown(Keys.D1))
                        {
                            vsg.AttachGameObjectToBone(att, "hand.R");
                        }
                        else
                        {
                            vsg.DetachGameObjectFromBone("hand.R");
                        }
                    }
                //    _lastKeyPress = WorldTime;
                //}
            }

            CurrentWorld.AddCameraRotationFromMouseDelta();

            int move = 0;
            int strafe = 0;
            if (Keyboard.IsKeyDown(Keys.W))
                move++;
            if (Keyboard.IsKeyDown(Keys.S))
                move--;
            if (Keyboard.IsKeyDown(Keys.A))
                strafe--;
            if (Keyboard.IsKeyDown(Keys.D))
                strafe++;
            
            if (move != 0 || strafe != 0)
            {
                MoveAndStrafeAlongCameraXZ(move, strafe, 0.025f);
                bobX = MathF.Sin(bobTime) * 0.25f;
                bobY = (bobZ - 1f) * 0.5f;
                bobZ = MathF.Sin(bobTime) * 1f;

                bobTime += 0.025f * 2;
            }
            else
            {
                bobTime = 0f;
                bobX *= 0.9f;
                bobY *= 0.9f;
                bobZ *= 0.9f;
                if (bobX < 0.00001f)
                    bobX = 0f;
                if (bobY < 0.00001f)
                    bobY = 0f;
                if (bobZ < 0.00001f)
                    bobZ = 0f;
            }
            

            foreach(Intersection i in GetIntersections<Wall>())
            {
                MoveOffset(i.MTV);
            }
            CurrentWorld.UpdateCameraPositionForFirstPersonView(Center, bobX * 0.125f * 0.5f, 0.5f + bobY * 0.125f * 1, bobZ * 0.125f * 0.5f * 0);
            if(vsg != null)
            {
                //vsw.SetOffset(0.25f, -0.25f, 0.75f);
                vsg.SetOffset(0.25f * 0 + bobX * 0.05f, -0.25f -bobY * 0.1f, 0 + bobZ * 0.1f);
                vsg.SetAnimationPercentageAdvance(0.005f);
            }
        }
    }
}
