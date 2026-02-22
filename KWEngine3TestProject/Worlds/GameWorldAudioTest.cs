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
        private int _loopId = -1;
        private float _cdOneShot = 0.25f;
        private float _lastOneShot = 0.0f;
        private float _lastP = 0f;

        public override void Act()
        {
            if(WorldTime - _lastP >= 2f)
            {
                ParticleObject po = new ParticleObject(3, ParticleType.BurstFire2);
                po.SetColorIntensity(1.5f);
                po.SetPosition(-2.5f, 0, 0);
                AddParticleObject(po);

                ParticleObject po2 = new ParticleObject(3, ParticleType.BurstFire2);
                po2.SetColorIntensity(1);
                po2.SetPosition(+2.5f, 0, 0);
                AddParticleObject(po2);

                _lastP = WorldTime;
            }

            if (Keyboard.IsKeyPressed(Keys.F1))
            {
                if (_loopId < 0)
                {
                    _loopId = Audio.PlaySound(@".\SFX\stage01_main.ogg", true, 0.5f);
                }
                else if(_loopId >= 0)
                {
                    if(Audio.IsChannelPaused(_loopId))
                    {
                        Audio.ContinueSound(_loopId);
                    }
                }

            }
            else if (Keyboard.IsKeyPressed(Keys.F2))
            {
                if (Audio.IsChannelPlayingOrPaused(_loopId))
                {
                    Audio.StopSound(_loopId);
                    _loopId = -1;
                }
            }
            else if (Keyboard.IsKeyPressed(Keys.F3))
            {
                Audio.PauseSound(_loopId);
            }
            else if(Keyboard.IsKeyDown(Keys.Space))
            {
                if (WorldTime - _lastOneShot > _cdOneShot)
                {
                    Audio.PlaySound(@".\SFX\shot01.ogg", false, 1.0f);
                    _lastOneShot = WorldTime;
                }
            }
        }

        public override void Prepare()
        {

        }
    }
}
