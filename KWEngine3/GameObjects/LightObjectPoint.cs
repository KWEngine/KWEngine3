using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für Punktlichter
    /// </summary>
    public class LightObjectPoint : LightObject
    {
        /// <summary>
        /// Konstruktormethode für die Erstellung einer Punktlichtinstanz
        /// </summary>
        /// <param name="shadowQuality">Schattenqualitätslevel</param>
        public LightObjectPoint(ShadowQuality shadowQuality)
        {
            Init(LightType.Directional, shadowQuality, ShadowType.Default);
        }
    }
}
