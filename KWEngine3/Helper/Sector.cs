//using KWEngine2.Collision;
using KWEngine3.Model;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal class Sector
    {
        internal bool IsValid { get; set; }
        public float Left { get; set; }
        public float Right { get; set; }
        public float Back { get;  set; }
        public float Front { get;  set; }

        public Vector2 Center { get;  set; }

        internal List<GeoTerrainTriangle> Triangles { get; set; }

        public Sector()
        {
            Left = 0;
            Right = 0;
            Back = 0;
            Front = 0;

            Center = new Vector2(0,0);

            IsValid = true; // for overhaul!
            Triangles = new List<GeoTerrainTriangle>();
        }
        public Sector(float l, float r, float b, float f)
        {
            Left = l;
            Right = r;
            Back = b;
            Front = f;

            Center = new Vector2((l + r) / 2f, (b + f) / 2f);

            IsValid = true;
            Triangles = new List<GeoTerrainTriangle>();
        }

        public void AddTriangle(GeoTerrainTriangle t)
        {
            if(Triangles.Contains(t) == false)
                Triangles.Add(t);
        }

        public GeoTerrainTriangle? GetTriangle(ref Vector3 untranslatedPosition)
        {

            foreach (GeoTerrainTriangle t in Triangles)
            {
                if (GeoTerrainTriangle.IsPointInTriangle(ref untranslatedPosition, ref t.Vertices[0], ref t.Vertices[1], ref t.Vertices[2]))
                {
                    return t;
                }
            }
            return null;
        }

        public string GetInfo()
        {
            string s = "L: " + Math.Round(Left, 2) + " | R: " + Math.Round(Right, 2) + " | F: " + Math.Round(Back, 2) + " | B: " + Math.Round(Front, 2);
            return s;
        }
    }
}
