using KWEngine3;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3.GameObjects;
using KWEngine3.Helper;

namespace KWEngine3TestProject.Classes.WorldFX
{
    internal class Player : GameObject
    {
        private Aura _aura;

        public void SetAura(Aura aura)
        {
            _aura = aura;
        }

        public Aura GetAura()
        {
            return _aura;
        }

        public override void Act()
        {
            Vector3 movement = Vector3.Zero;
            if (Keyboard.IsKeyDown(Keys.A))
                movement.X -= 1f;
            if (Keyboard.IsKeyDown(Keys.D))
                movement.X += 1f;
            if (Keyboard.IsKeyDown(Keys.W))
                movement.Z -= 1f;
            if (Keyboard.IsKeyDown(Keys.S))
                movement.Z += 1f;

            if (movement.LengthSquared > 0)
            {
                SetAnimationID(6);
                SetAnimationPercentageAdvance(0.006f);
                MoveAlongVector(Vector3.NormalizeFast(movement), 0.05f);
            }
            else
            {
                SetAnimationID(0);
                SetAnimationPercentageAdvance(0.0025f);
            }
            
            
            UpdateAuraPosition();
        }

        private void UpdateAuraPosition()
        {
            if(this._aura != null)
            {
                this._aura.SetPosition(this.Position.X, 0.0125f, this.Position.Z);
            }
        }
    }
}
