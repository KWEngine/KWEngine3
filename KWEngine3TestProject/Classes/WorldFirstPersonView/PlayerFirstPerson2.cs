using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldPlaneMTVCollider;
using KWEngine3TestProject.Worlds;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldFirstPersonView
{
    internal class PlayerFirstPerson2 : GameObject
    {
        private float bobZ = 0f;
        private float bobY = 0f;
        private float bobX = 0f;
        private float bobTime = 0f;

        public override void Act()
        {
            if (Keyboard.IsKeyPressed(Keys.F5))
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

            
            ViewSpaceGameObject vsg = CurrentWorld.GetViewSpaceGameObject();
            if (vsg != null && (Keyboard.IsKeyPressed(Keys.D1) || Keyboard.IsKeyPressed(Keys.D2)))
            {

                Immovable att = CurrentWorld.GetGameObjectByName<Immovable>("Gun");
                if (att != null)
                {
                    if (Keyboard.IsKeyDown(Keys.D1))
                    {
                        vsg.AttachGameObjectToBone(att, "Weapon_R");
                        HelperGameObjectAttachment.SetScaleForAttachment(att, 0.2f);
                        HelperGameObjectAttachment.SetPositionOffsetForAttachment(att, 0, -0.01f, -0.005f);
                        HelperGameObjectAttachment.SetRotationForAttachment(att, 90, 180, 0);
                    }
                    else
                    {
                        vsg.DetachGameObjectFromBone("Weapon_R");
                    }
                }

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
                bobX = MathF.Sin(bobTime * 4) * 0.25f * MathF.Min(bobTime, 1f);
                bobZ = MathF.Sin(bobTime * 4) * 0.5f * MathF.Min(bobTime, 1f);
                bobY = (bobZ - 1f) * 0.5f * MathF.Min(bobTime, 1f);

                bobTime += 0.0125f;
            }
            else
            {
                bobTime *= 0.99f;
                bobX *= 0.99f;
                bobY *= 0.99f;
                bobZ *= 0.99f;
                if (Math.Abs(bobX) < 0.00001f)
                    bobX = 0f;
                if (Math.Abs(bobY) < 0.00001f)
                    bobY = 0f;
                if (Math.Abs(bobZ) < 0.00001f)
                    bobZ = 0f;
            }


            foreach (Intersection i in GetIntersections<Wall>())
            {
                MoveOffset(i.MTV);
            }
            CurrentWorld.UpdateCameraPositionForFirstPersonView(Center, bobX * 0.125f * 0.5f, 0.5f + bobY * 0.125f * 1, bobZ * 0.125f * 0.5f * 0);

            if (vsg != null)
            {
                //vsw.SetOffset(0.25f, -0.25f, 0.75f);
                vsg.SetOffset(0.25f * 0 + bobX * 0.05f, -0.25f - bobY * 0.1f, 0.09f + bobZ * 0.1f);
                vsg.SetAnimationPercentageAdvance(0.005f);
            }

            Console.WriteLine($"bobx: {bobX}, boby: {bobY}, bobz: {bobZ}");
        }
    }
}
