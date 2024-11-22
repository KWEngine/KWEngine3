using KWEngine3.GameObjects;
using KWEngine3.Helper;
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
        //private const int NORMALCOUNT = 30;
        //private List<Vector3> _surfaceNormals = new();
        private int mode = 1; // 0 = stand, 1 = fall
        private float velocityY = 0;
        
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

            RayTerrainIntersection rti = RaytraceTerrainBelowPosition(new Vector3(Position.X, AABBLow, Position.Z));
            if(rti.IsValid)
            {
                if (mode == 1)
                {
                    if (rti.Distance < 0.05f)
                    {
                        mode = 0;
                        SetPositionY(rti.IntersectionPoint.Y, KWEngine3.PositionMode.BottomOfAABBHitbox);
                    }
                }
                else
                {
                    SetPositionY(rti.IntersectionPoint.Y, KWEngine3.PositionMode.BottomOfAABBHitbox);
                }
            }

            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            
            if (movementVector.LengthSquared > 0)
            {
                movementVector.NormalizeFast();
                TurnTowardsXZ(Position + movementVector);
                MoveAlongVector(movementVector, 0.01f);
            }












            /*
            RayTerrainIntersectionSet rti = RaytraceTerrainBelowPositionExt(GetOBBBottom(), KWEngine3.RayMode.FourRaysY, 1f);
            if (rti.IsValid)
            {
                //Console.WriteLine(rti.IntersectionPointAvg.Y);
                SetPositionY(rti.IntersectionPointAvg.Y, KWEngine3.PositionMode.BottomOfAABBHitbox);
                if(_surfaceNormals.Count >= NORMALCOUNT)
                {
                    _surfaceNormals.RemoveAt(0); 
                }
                _surfaceNormals.Add(rti.SurfaceNormalAvg);
            }
            else
            {
                //Console.WriteLine("no ray collision detected");
            }

            if(_surfaceNormals.Count == NORMALCOUNT)
            {
                Vector3 avg = Vector3.Zero;
                foreach(Vector3 n in _surfaceNormals)
                {
                    avg += n;
                }
                avg /= NORMALCOUNT;
                SetRotationToMatchSurfaceNormal(Vector3.Normalize(avg));
            }
            */
        }
    }
}
