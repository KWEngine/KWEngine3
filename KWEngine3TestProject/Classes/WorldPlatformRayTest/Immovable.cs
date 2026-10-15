using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections;

namespace KWEngine3TestProject.Classes.WorldPlatformRayTest
{
    internal class Immovable : GameObject
    {
        public Immovable()
        {
            FlowFieldCost = 255;
        }
        public override void Act()
        {

        }
    }
}
