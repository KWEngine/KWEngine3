using KWEngine3.GameObjects;
using OpenTK.Mathematics;

namespace KWEngine3.Helper
{
    /// <summary>
    /// Helferklasse für GameObject-Instanzen, die an deren GameObject-Instanzen angeheftet sind
    /// </summary>
    public static class HelperGameObjectAttachment
    {
        /// <summary>
        /// Setzt den relativen Abstand zum Elternobjekt
        /// </summary>
        /// <param name="attachedObject">Elternobjekt</param>
        /// <param name="x">x-Abstand</param>
        /// <param name="y">y-Abstand</param>
        /// <param name="z">z-Abstand</param>
        public static void SetPositionOffsetForAttachment(GameObject attachedObject, float x, float y, float z)
        {
            SetPositionOffsetForAttachment(attachedObject, new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt den relativen Abstand zum Elternobjekt
        /// </summary>
        /// <param name="attachedObject">Elternobjekt</param>
        /// <param name="offset">Abstand</param>
        public static void SetPositionOffsetForAttachment(GameObject attachedObject, Vector3 offset)
        {
            attachedObject._positionOffsetForAttachment = offset;
        }

        /// <summary>
        /// Setzt die Größe des angehefteten Objekts, nachdem es an das Elternobjekt angeheftet wurde
        /// </summary>
        /// <param name="attachedObject">Angeheftetes Objekt</param>
        /// <param name="s">Gleichmäßige Skalierung in alle drei Achsenrichtungen</param>
        public static void SetScaleForAttachment(GameObject attachedObject, float s)
        {
            SetScaleForAttachment(attachedObject, new Vector3(s, s, s));
        }

        /// <summary>
        /// Setzt die Größe des angehefteten Objekts, nachdem es an das Elternobjekt angeheftet wurde
        /// </summary>
        /// <param name="attachedObject">Angeheftetes Objekt</param>
        /// <param name="x">Skalierung in x-Richtung</param>
        /// <param name="y">Skalierung in y-Richtung</param>
        /// <param name="z">Skalierung in z-Richtung</param>
        public static void SetScaleForAttachment(GameObject attachedObject, float x, float y, float z)
        {
            SetScaleForAttachment(attachedObject, new Vector3(x, y, z));
        }

        /// <summary>
        /// Setzt die Größe des angehefteten Objekts, nachdem es an das Elternobjekt angeheftet wurde
        /// </summary>
        /// <param name="attachedObject">Angeheftetes Objekt</param>
        /// <param name="scale">Skalierung</param>
        public static void SetScaleForAttachment(GameObject attachedObject, Vector3 scale)
        {
            if (scale.X <= 0 || scale.Y <= 0 || scale.Z <= 0)
                scale = Vector3.One;
            attachedObject._scaleOffsetForAttachment = scale;
        }

        /// <summary>
        /// Setzt die Rotation des angehefteten Objekts, nachdem es an das Elternobjekt angeheftet wurde
        /// </summary>
        /// <param name="attachedObject">Angeheftetes Objekt</param>
        /// <param name="x">Skalierung um die lokale x-Achse in Grad</param>
        /// <param name="y">Skalierung um die lokale y-Achse in Grad</param>
        /// <param name="z">Skalierung um die lokale z-Achse in Grad</param>
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
