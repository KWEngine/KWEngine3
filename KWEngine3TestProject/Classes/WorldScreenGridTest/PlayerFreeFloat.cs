using KWEngine3;
using KWEngine3.GameObjects;
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace KWEngine3TestProject.Classes.WorldScreenGridTest
{
    class PlayerFreeFloat : GameObject
    {
        private float _speed = 0.05f;

        public PlayerFreeFloat()
        {
            SkipRender = true;
            IsShadowCaster = false;
            SetRotation(0, 180, 0);
            KWEngine.CurrentWorld.SetCameraToFirstPersonGameObject(this, 0);
        }

        public override void Act()
        {
            CurrentWorld.AddCameraRotationFromMouseDelta();

            int move = 0;
            int strafe = 0;
            if (Keyboard.IsKeyDown(Keys.W))
                move++;
            if (Keyboard.IsKeyDown(Keys.S))
                move--;
            if (Keyboard.IsKeyDown(Keys.A))
                strafe--;
            if (Keyboard.IsKeyDown(Keys.D))
                strafe++;

            if (Keyboard.IsKeyDown(Keys.E))
                MoveAlongVector(CurrentWorld.CameraLookAtVectorLocalUp, _speed);
            if (Keyboard.IsKeyDown(Keys.Q))
                MoveAlongVector(-CurrentWorld.CameraLookAtVectorLocalUp, _speed);

            if (move != 0 || strafe != 0)
            {
                MoveAndStrafeAlongCamera(move, strafe, _speed);
            }

            CurrentWorld.UpdateCameraPositionForFirstPersonView(Center, 0f);
        }
    }
}
