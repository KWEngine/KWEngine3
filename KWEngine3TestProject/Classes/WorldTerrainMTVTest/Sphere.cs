using KWEngine3.GameObjects;
using KWEngine3.Helper;
using KWEngine3.Model;
using KWEngine3TestProject.Classes.WorldTerrainMTVTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes
{
    internal class Sphere : GameObject
    {
        //private List<ContactPoint> _contactPoints = new(100);

        public Sphere()
        {
            /*
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
            */
        }

        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.W))
                MoveOffset(0, 0, -0.005f);
            if (Keyboard.IsKeyDown(Keys.S))
                MoveOffset(0, 0, +0.005f);
            if (Keyboard.IsKeyDown(Keys.A))
                MoveOffset(-0.005f, 0, 0);
            if (Keyboard.IsKeyDown(Keys.D))
                MoveOffset(+0.005f, 0, 0);
            if (Keyboard.IsKeyDown(Keys.Q))
                MoveOffset(0, -0.005f, 0);
            if (Keyboard.IsKeyDown(Keys.E))
                MoveOffset(0, +0.005f, 0);

            IntersectionTerrain it = GetIntersectionWithTerrain();
            if(it != null)
            {
                MoveOffset(it.MTV);
            }

            /*
            List<Vector3> pointsOnTriangles = new();
            foreach(GeoTerrainTriangle tri in contactPoints.Keys)
            {
                pointsOnTriangles.AddRange(contactPoints[tri]);
            }
            for (int i = 0; i < _contactPoints.Count; i++)
            {

                if (i < pointsOnTriangles.Count)
                {
                    _contactPoints[i].SetOpacity(1);
                    _contactPoints[i].SetPosition(pointsOnTriangles[i]);
                }
                else
                {
                    _contactPoints[i].SetOpacity(0);
                }
            }
            */

            CurrentWorld.SetCameraPosition(this.Center + new Vector3(0, 0.5f, 2.5f));
            //CurrentWorld.SetCameraPosition(this.Center + new Vector3(0, 7.5f, 10f));
            CurrentWorld.SetCameraTarget(this.Center);
        }
    }
}
