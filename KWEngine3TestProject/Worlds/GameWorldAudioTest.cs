using KWEngine3;
using KWEngine3.Audio;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldAudioTest : World
    {
        private float _cd = 0.5f;
        private float _last = 0.0f;
        private int _loopId = -1;
        public override void Act()
        {
            if (WorldTime - _last > _cd)
            {
                if (Keyboard[Keys.F1])
                {
                    _loopId = Audio.PlaySound(@".\SFX\stage01_main.ogg", true, 0.5f);
                    _last = WorldTime;
                }
                else if (Keyboard[Keys.F2])
                {
                    Audio.StopSound(_loopId);
                    _last = WorldTime;
                }


            }
        }

        public override void Prepare()
        {

        }
    }
}
