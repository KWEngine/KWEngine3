using KWEngine3;
using KWEngine3TestProject.Worlds;

namespace KWEngine3TestProject
{
    public class GameWindow : GLWindow
    {
        public GameWindow() : base(
            1280,                           // Window width
            720,                            // Window height
            false,                           // VSync on?
            PostProcessingQuality.Standard, // Quality level (Standard recommended for iGPUs)
            WindowMode.Default              // Window mode
            )
        {
            SetWorld(new GameWorldMultipleTerrainTest());
        }
    }
}
