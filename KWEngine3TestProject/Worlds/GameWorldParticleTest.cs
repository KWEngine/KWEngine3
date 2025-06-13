using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes.WorldParticleTest;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldParticleTest : World
    {
        private float _time = 0f;
        public override void Act()
        {
            if(_time == 0f || WorldTime - _time > 3f)
            {
                ParticleObject p = new ParticleObject(2, ParticleType.BurstFire1);
                p.SetPosition(0, 3, 0);
                p.SetHue(200);
                AddParticleObject(p);

                _time = WorldTime;
            }
        }

        public override void Prepare()
        {
            SetCameraFOV(10);
            SetCameraPosition(0, 2, 50);
            SetCameraTarget(0, 2, 0);
            SetColorAmbient(1f, 1f, 1f);
            SetBackground2D("./Textures/greenmountains.dds");

            Player p = new Player();
            p.Name = "Player #1";
            p.SetModel("KWQuad2D");
            p.SetTexture("./Textures/spritesheet2.png");
            p.IsCollisionObject = true;
            p.HasTransparencyTexture = true;
            p.BlendTextureStates = false;
            p.SetTextureRepeat(1f / 7f, 1f / 3f);
            p.SetTextureOffset(0, 0);
            p.SetTextureClip(0.333f, 0.15f);
            AddGameObject(p);

            Floor f01 = new Floor();
            f01.SetModel("KWQuad");
            f01.SetPosition(0, -1, 0);
            f01.SetScale(20, 2, 1);
            f01.IsCollisionObject = true;
            f01.SetTexture("./Textures/building_albedo.jpg");
            f01.SetTexture("./Textures/building_emissive.jpg", TextureType.Emissive);
            f01.SetTextureRepeat(10, 1);
            AddGameObject(f01);
        }
    }
}
