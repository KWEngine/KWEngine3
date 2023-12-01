using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldHitboxConcurrencyTest
{
    internal class Player : GameObject
    {
        private readonly float _cooldown = 0.1f;
        private float _timestampLastShot = 0;
        public override void Act()
        {
            TurnTowardsXZ(HelperIntersection.GetMouseIntersectionPointOnPlane(Plane.XZ, 0));

            if(Keyboard.IsKeyDown(Keys.A))
            {
                MoveOffset(-0.03f, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                MoveOffset(+0.03f, 0, 0);
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                MoveOffset(0, 0, -0.03f);
            }
            if (Keyboard.IsKeyDown(Keys.S))
            {
                MoveOffset(0, 0, +0.03f);
            }

            if(Mouse.IsButtonDown(MouseButton.Left) && WorldTime - _timestampLastShot >= _cooldown)
            {;
                ShotPlayer sp = new ShotPlayer();
                sp.SetPosition(this.Position + this.LookAtVector * 1.0f);
                sp.SetRotation(this.Rotation);
                sp.SetScale(0.1f, 0.1f, 1.25f);
                sp.SetColorEmissive(0, 1, 0, 5);
                sp.IsCollisionObject = true;
                CurrentWorld.AddGameObject(sp);

                _timestampLastShot = WorldTime;
            }
        }
    }
}
