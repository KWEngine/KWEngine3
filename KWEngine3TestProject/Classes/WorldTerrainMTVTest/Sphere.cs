using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes.WorldTerrainMTVTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes
{
    internal class Sphere : GameObject
    {
        private List<ContactPoint> _contactPoints = new(100);

        public Sphere()
        {
            for (int i = 0; i < 100; i++)
            {
                ContactPoint cp = new ContactPoint();
                cp.SetModel("KWSphere");
                cp.SetColor(1, 0, 0);
                cp.SetOpacity(0);
                cp.SetScale(0.1f);
                cp.IsAffectedByLight = false;
                cp.IsShadowCaster = false;
                _contactPoints.Add(cp);
                CurrentWorld.AddGameObject(cp);
            }
        }

        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.W))
                MoveOffset(0, 0, -0.01f);
            if (Keyboard.IsKeyDown(Keys.S))
                MoveOffset(0, 0, +0.01f);
            if (Keyboard.IsKeyDown(Keys.A))
                MoveOffset(-0.01f, 0, 0);
            if (Keyboard.IsKeyDown(Keys.D))
                MoveOffset(+0.01f, 0, 0);
            if (Keyboard.IsKeyDown(Keys.Q))
                MoveOffset(0, -0.01f, 0);
            if (Keyboard.IsKeyDown(Keys.E))
                MoveOffset(0, +0.01f, 0);

            /*
            List<IntersectionTerrain> intersections = GetIntersectionsWithTerrain();
            
            int maxIterations = 8;
            if (intersections.Count >= 1)
            {
                for (int i = 1; i <= maxIterations; i++)
                {
                    Vector3 mtv = HelperIntersection.CalculateWeightedTerrainMTV(intersections);
                    float ii = i;
                    MoveOffset(mtv * (ii / maxIterations));
                    intersections = GetIntersectionsWithTerrain();
                    if (intersections.Count == 0)
                        break;
                }
            }
            */
            List<Vector3> contactPoints = SolveIntersectionsWithTerrain();
            for(int i = 0; i < _contactPoints.Count; i++)
            {
                if (i < contactPoints.Count)
                {
                    _contactPoints[i].SetOpacity(1);
                    _contactPoints[i].SetPosition(contactPoints[i]);
                }
                else
                {
                    _contactPoints[i].SetOpacity(0);
                }
            }


            //CurrentWorld.SetCameraPosition(this.Center + new Vector3(0, 0.5f, 2.5f));
            //CurrentWorld.SetCameraPosition(this.Center + new Vector3(0, 7.5f, 10f));
            //CurrentWorld.SetCameraTarget(this.Center);
        }
    }
}
