using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3.Helper;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;


namespace KWEngine3TestProject.Classes.WorldFirstPersonView
{
    public class PlayerFirstPerson : GameObject
    {
        private float _lastKeyPress = float.MinValue;

        public override void Act()
        {
            ViewSpaceGameObject vsg = CurrentWorld.GetViewSpaceGameObject();
            if (vsg != null && (Keyboard.IsKeyDown(Keys.D1) || Keyboard.IsKeyDown(Keys.D2)))
            {
                if (WorldTime - _lastKeyPress > 0.125f)
                {
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
                    _lastKeyPress = WorldTime;
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
            }

            CurrentWorld.UpdateCameraPositionForFirstPersonView(Center, 0.5f);
        }
    }
}
