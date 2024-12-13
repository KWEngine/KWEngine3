using KWEngine3;
using KWEngine3TestProject.Worlds;

namespace KWEngine3TestProject
{
    public class GameWindow : GLWindow
    {
        public GameWindow() : base(
            //1920,                           // Window width
            //1080,                            // Window height
            true,                           // VSync on?
            PostProcessingQuality.Standard // Quality level (Standard recommended for iGPUs)
            //WindowMode.BorderlessWindow              // Window mode
            )
        {
            SetWorld(new GameWorldGPUDebug());
        }
    }
}
