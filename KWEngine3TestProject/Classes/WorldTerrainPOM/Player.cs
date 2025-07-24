using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldTerrainPOM
{
    internal class Player : GameObject
    {
        private float _speed = 0.02f;

        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.A))
                MoveOffset(-_speed, 0, 0);
            if (Keyboard.IsKeyDown(Keys.D))
                MoveOffset(+_speed, 0, 0);
            if (Keyboard.IsKeyDown(Keys.W))
                MoveOffset(0, 0, -_speed);
            if (Keyboard.IsKeyDown(Keys.S))
                MoveOffset(0, 0, +_speed);

            
            RayTerrainIntersection rti = RaytraceTerrainBelowPosition(this.GetOBBBottom());
            if(rti.IsValid)
            {
                SetRotationToMatchSurfaceNormal(rti.SurfaceNormal);
                SetPositionY(rti.IntersectionPoint.Y, PositionMode.BottomOfAABBHitbox);
            }
            /*
            if(HelperIntersection.GetPositionOnTerrainBelow(this.GetOBBBottom(), out Vector3 ip, out float distance))
            {
                Console.WriteLine(ip);
                Console.WriteLine(distance);
            }
            */
        }
    }
}
