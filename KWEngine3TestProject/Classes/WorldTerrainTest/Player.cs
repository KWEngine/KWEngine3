﻿using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTerrainTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            Vector3 movementVector = new Vector3(0, 0, 0);

            if (Keyboard.IsKeyDown(Keys.W))
                movementVector.Z -= 1;
            if (Keyboard.IsKeyDown(Keys.S))
                movementVector.Z += 1;
            if (Keyboard.IsKeyDown(Keys.A))
                movementVector.X -= 1;
            if (Keyboard.IsKeyDown(Keys.D))
                movementVector.X += 1;

            if (Keyboard.IsKeyDown(Keys.Q))
                MoveOffset(0, -0.01f, 0);
            if (Keyboard.IsKeyDown(Keys.E))
                MoveOffset(0, +0.01f, 0);

            if (movementVector.LengthSquared > 0)
            {
                movementVector.NormalizeFast();
                TurnTowardsXZ(Position + movementVector);
                MoveAlongVector(movementVector, 0.01f);
            }

            IntersectionTerrain it = GetIntersectionWithTerrain();
            if (it != null)
            {
                MoveOffset(it.MTV);
            }
        }
    }
}