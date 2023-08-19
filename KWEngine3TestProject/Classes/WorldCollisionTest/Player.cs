using KWEngine3.GameObjects;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;

namespace KWEngine3TestProject.Classes.WorldCollisionTest
{
    internal class Player : GameObject
    {
        public override void Act()
        {
            if (Keyboard[Keys.R])
            {
                SetRotation(0, 0, 0);
            }

            if(Keyboard[Keys.W])
            {
                MoveOffset(0, 0.01f, 0);
            }
            if (Keyboard[Keys.S])
            {
                MoveOffset(0, -0.01f, 0);
            }

            Intersection i = GetIntersection();
            if (i != null)
            {
                this.SetRotationToMatchSurfaceNormal(i.ColliderSurfaceNormal);
            }
        }
    }
}
