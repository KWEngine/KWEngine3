using KWEngine3;
using KWEngine3.GameObjects;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.World2DLAVTest
{
    internal class Player : GameObject
    {
        private LAV _lav;

        public Player()
        {
            _lav = new LAV();
            _lav.SetColor(0, 1, 0);
            _lav.SetScale(0.1f, 0.1f, 1.0f);
            CurrentWorld.AddGameObject(_lav);
        }

        public override void Act()
        {
            if (Keyboard.IsKeyDown(Keys.A)) MoveOffset(-0.01f, +0.00f, 0);
            if (Keyboard.IsKeyDown(Keys.D)) MoveOffset(+0.01f, +0.00f, 0);
            if (Keyboard.IsKeyDown(Keys.S)) MoveOffset(+0.00f, -0.01f, 0);
            if (Keyboard.IsKeyDown(Keys.W)) MoveOffset(+0.00f, +0.01f, 0);

            Target t = CurrentWorld.GetGameObjectByName<Target>("T");
            if (t != null)
                TurnTowardsXY(t.Position);

            if (_lav != null)
            {
                _lav.SetRotation(this.Rotation);
                _lav.SetPosition(this.Position + _lav.LookAtVector * 0.5f);
                
            }
        }
    }
}
