using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldTerrainTest
{
    internal class Player : GameObject
    {
        //private const int NORMALCOUNT = 30;
        //private List<Vector3> _surfaceNormals = new();
        private int mode = 1; // 0 = stand, 1 = fall
        private float velocityY = 0;
        
        public override void Act()
        {
            if(Keyboard.IsKeyPressed(Keys.Space) && mode == 0)
            {
                mode = 1;
                velocityY = 0.1f;
            }

            if(mode == 1)
            {
                velocityY -= 0.001f;
                velocityY = Math.Max(-0.2f, velocityY);
                MoveOffset(0, velocityY, 0);
            }

            /*
            RayTerrainIntersection rti = RaytraceTerrainBelowPosition(
                //new Vector3(Position.X, AABBLow, Position.Z)
                this.GetOBBBottom()
                );
            if(rti.IsValid)
            {
                if (mode == 1)
                {
                    if (rti.Distance < 0.05f)
                    {
                        mode = 0;
                        SetPositionY(rti.IntersectionPoint.Y, PositionMode.BottomOfAABBHitbox);
                    }
                }
                else
                {
                    SetPositionY(rti.IntersectionPoint.Y, PositionMode.BottomOfAABBHitbox);
                }
            }
            */

            
            RayTerrainIntersectionSet result = RaytraceTerrainBelowPositionExt(GetOBBBottom(), RayMode.FiveRaysY, 1f);
            if (result.IsValid)
            {
                SetPositionY(result.IntersectionPointAvg.Y, PositionMode.BottomOfAABBHitbox);
                SetRotationToMatchSurfaceNormal(result.SurfaceNormalAvg);
            }
            else
            {
                //Console.WriteLine("no ray collision detected");
            }



            if(Keyboard.IsKeyDown(Keys.A))
            {
                AddRotationY(1, true);
            }
            if (Keyboard.IsKeyDown(Keys.D))
            {
                AddRotationY(-1, true);
            }
            if (Keyboard.IsKeyDown(Keys.W))
            {
                Move(0.01f);
            }
            if(Keyboard.IsKeyDown(Keys.S))
            {
                Move(-0.01f);
            }
        }
    }
}
