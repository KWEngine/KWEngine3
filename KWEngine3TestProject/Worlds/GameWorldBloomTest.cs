using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3TestProject.Classes;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Worlds
{
    internal class GameWorldBloomTest : World
    {
        private float _timestampLastExplosion = 0;
        private float _timestampLastParticle = 0;
        public override void Act()
        {
           if(WorldTime - _timestampLastExplosion > 1.5f)
           {
                ExplosionObject e = new ExplosionObject(64, 0.5f, 5f, 1f, ExplosionType.Cube);
                e.SetColorEmissive(1, 1, 1, 2);
                e.SetPosition(-5, 8, 0);
                AddExplosionObject(e);
                _timestampLastExplosion = WorldTime;
           }

            if (WorldTime - _timestampLastParticle > 5f)
            {
                ParticleObject p = new ParticleObject(4, ParticleType.BurstFire3);
                p.SetPosition(8, 8, 0);
                p.SetColor(1, 1, 1, 0.75f);
                AddParticleObject(p);
                _timestampLastParticle = WorldTime;
            }
        }

        public override void Prepare()
        {
            SetBackground2D(@"./Textures/greenmountains.dds");
            SetCameraFOV(90);
            SetCameraPosition(0, 0, 25);

            Immovable i01 = new Immovable();
            i01.SetPosition(-6, 0, 0);
            i01.SetScale(0.25f, 0.1f, 0.1f);
            i01.SetColor(1, 0.5f, 0.25f);
            i01.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i01);

            Immovable i02 = new Immovable();
            i02.SetPosition(-4, 0, 0);
            i02.SetScale(0.5f, 0.1f, 0.1f);
            i02.SetColor(1, 0.5f, 0.25f);
            i02.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i02);

            Immovable i03 = new Immovable();
            i03.SetPosition(-2, 0, 0);
            i03.SetScale(0.1f, 0.5f, 0.1f);
            i03.SetColor(1, 0.5f, 0.25f);
            i03.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i03);

            Immovable i04 = new Immovable();
            i04.SetPosition(0, 0, 0);
            i04.SetScale(0.5f, 0.5f, 0.1f);
            i04.SetColor(1, 0.5f, 0.25f);
            i04.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i04);

            Immovable i05 = new Immovable();
            i05.SetPosition(3, 0, 0);
            i05.SetScale(3f, 3f, 3f);
            i05.SetColor(1, 0.5f, 0.25f);
            i05.SetColorEmissive(1, 0.5f, 0.25f, 5f);
            AddGameObject(i05);

            HUDObjectText t1 = new HUDObjectText("a TEST this is.");
            t1.SetColor(1, 0, 1);
            t1.SetPosition(Window.Center.X, Window.Center.Y / 3);
            t1.SetColorEmissive(1, 1, 1);
            t1.SetColorEmissiveIntensity(1);
            AddHUDObject(t1);
        }
    }
}
