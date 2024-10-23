using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    internal struct HelperMouseRay
    {
        private Vector3 mStart;
        private Vector3 mEnd;

        public Vector3 Start { get { return mStart; } }

        public Vector3 End { get { return mEnd; } }

        public HelperMouseRay(float x, float y, Matrix4 viewMatrix, Matrix4 projectionMatrix)
        {
            mStart = HelperGeneral.UnProject(new Vector3(x, y, 0.0f), projectionMatrix, viewMatrix, KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y);
            mEnd = HelperGeneral.UnProject(new Vector3(x, y, 1.0f), projectionMatrix, viewMatrix, KWEngine.Window.ClientRectangle.Size.X, KWEngine.Window.ClientRectangle.Size.Y);
        }
    }
}
