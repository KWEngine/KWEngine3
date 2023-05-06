using KWEngine3.GameObjects;
using KWEngine3;
using KWEngine3.Helper;
using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldPlatformerPack
{
    public class Weapon : GameObject
    {
        private Vector3 _pivot = Vector3.Zero;
        public void SetPivot(float x, float y, float z)
        {
            _pivot.X = x;
            _pivot.Y = y;
            _pivot.Z = z;
        }

        public override void Act()
        {
            if (IsAttachedToGameObject == false)
            {
                float y = (float)Math.Sin(CurrentWorld.WorldTime);
                SetPosition(_pivot + new Vector3(0, y * 0.25f, 0));

                AddRotationY(0.5f + Math.Abs(y));
            }
            
        }
    }
}
