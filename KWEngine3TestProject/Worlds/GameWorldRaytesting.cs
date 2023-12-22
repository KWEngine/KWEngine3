using KWEngine3;
using KWEngine3.Helper;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldRaytesting : World
    {
        private Pointer _sphere;
        private Immovable _floor;
        private Vector3 _rayStart = new Vector3(-2, 20, 0);

        public override void Act()
        {
            List<RayIntersection> hits = HelperIntersection.RayTraceObjectsForViewVectorFast(_rayStart, -Vector3.UnitY, null, 0, true, typeof(Immovable));
            if(hits.Count == 0)
            {
                _sphere.SetOpacity(0);
                //Console.WriteLine("no ray intersection");
            }
            else
            {
                float d = hits[0].Distance;
                Console.WriteLine("ray intersection: " + hits[0].IntersectionPoint + " (distance: " + d + ")");
                _sphere.SetOpacity(1);
                _sphere.SetPosition(hits[0].IntersectionPoint);
            }
            _floor.AddRotationZ(0.01f);
            _floor.AddRotationX(0.01f);

        }

        public override void Prepare()
        {
            SetCameraPosition(0, 10, 15);

            _sphere = new Pointer();
            _sphere.SetModel("KWSphere");
            _sphere.SetScale(0.25f);
            _sphere.SetColor(1, 1, 0);
            _sphere.SetColorEmissive(1, 0, 0, 5);
            _sphere.SetOpacity(0);
            AddGameObject(_sphere);

            _floor = new Immovable();
            _floor.SetScale(20, 2, 10);
            _floor.SetColor(1, 1, 1);
            _floor.SetTexture("./Textures/Tile_01_512.png");
            _floor.SetTextureRepeat(10, 5);
            _floor.SetRotation(0, 0, 45 );
            AddGameObject(_floor);

        }
    }
}
