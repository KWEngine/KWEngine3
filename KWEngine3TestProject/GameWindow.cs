﻿using KWEngine3;
using KWEngine3TestProject.Worlds;

namespace KWEngine3TestProject
{
    public class GameWindow : GLWindow
    {
        public GameWindow() : base(
            1280, 
            720, 
            true, 
            PostProcessingQuality.Standard, 
            WindowMode.Default
            )
        {
            SetWorld(new GameWorldEmpty());
        }
    }
}
