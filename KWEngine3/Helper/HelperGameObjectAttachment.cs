using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    public static class HelperGameObjectAttachment
    {
        public static void SetPositionOffsetForAttachment(GameObject attachedObject, float x, float y, float z)
        {
            SetPositionOffsetForAttachment(attachedObject, new Vector3(x, y, z));
        }

        public static void SetPositionOffsetForAttachment(GameObject attachedObject, Vector3 offset)
        {
            attachedObject._positionOffsetForAttachment = offset;
        }

        public static void SetScaleForAttachment(GameObject attachedObject, float x, float y, float z)
        {
            SetScaleForAttachment(attachedObject, new Vector3(x, y, z));
        }

        public static void SetScaleForAttachment(GameObject attachedObject, Vector3 scale)
        {
            if (scale.X == 0 || scale.Y == 0 || scale.Z == 0)
                scale = Vector3.One;
            attachedObject._scaleOffsetForAttachment = scale;
        }

        public static void SetRotationForAttachment(GameObject attachedObject, float x, float y, float z)
        {
            attachedObject._rotationOffsetForAttachment = Quaternion.FromEulerAngles(
                MathHelper.DegreesToRadians(x), 
                MathHelper.DegreesToRadians(y), 
                MathHelper.DegreesToRadians(z)
                );
        }
    }
}
