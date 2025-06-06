﻿using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes
{
    internal class Pointer : GameObject
    {
        public override void Act()
        {
            /*List<RayIntersectionExt> intersections = HelperIntersection.RayTraceObjectsForViewVector(
                HelperIntersection.GetMouseOrigin(), 
                HelperIntersection.GetMouseRay(), 
                0, 
                true,
                this,
                typeof(GameObject)
                );
            */
            /*
            List<RayIntersection> intersections = HelperIntersection.RayTraceObjectsForViewVectorFastest(
                HelperIntersection.GetMouseOrigin(),
                HelperIntersection.GetMouseRay(), this, 0, true, typeof(GameObject)
                );
            */
            List<RayIntersection> intersections = HelperIntersection.RayTraceObjectsForViewVectorFast(
                HelperIntersection.GetMouseOrigin(),
                HelperIntersection.GetMouseRay(), this, 0, true, typeof(GameObject)
                );

            if (intersections.Count > 0)
            {
                SetPosition(intersections[0].IntersectionPoint);
            }
        }
    }
}
