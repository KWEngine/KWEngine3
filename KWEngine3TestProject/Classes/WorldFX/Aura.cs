using KWEngine3;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using KWEngine3.GameObjects;
using KWEngine3.Helper;

namespace KWEngine3TestProject.Classes.WorldFX
{
    internal class Aura : GameObject
    {
        private bool _enabled;

        public Aura()
        {
            this.SetModel("Aura");
            this.SetEnabled(true);
        }

        public void SetEnabled(bool enabled)
        {
            this._enabled = enabled;
            this.SetOpacity(this._enabled ? 1f : 0f, 0);
            this.SetOpacity(this._enabled ? 1f : 0f, 1);
        }

        public override void Act()
        {
            HelperRotation.SetMeshPreRotationYZX(this, 0, 0, MathF.Sin(WorldTime) * 360f, 0);
            //HelperRotation.SetMeshPreRotationYZX(this, 1, 0, MathF.Cos(WorldTime) * 360f, 0);
        }
    }
}
