using System;
using System.Collections.Generic;
using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject
{
    public class GameWorldStart : World
    {
        public override void Act()
        {
            
        }

        public override void Prepare()
        {
            SetBackground2D("./Textures/Trauersmiley.png");
            SetBackgroundFillColor(1, 0, 0);
        }
    }
}
