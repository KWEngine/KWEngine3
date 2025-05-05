using OpenTK.Mathematics;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für Lichter mit gerichtetem Lichtkegel
    /// </summary>
    public class LightObjectDirectional : LightObject
    {
        /// <summary>
        /// Konstruktormethode für die Erstellung einer Lichtinstanz mit gerichtetem Lichtkegel
        /// </summary>
        /// <param name="shadowQuality">Schattenqualitätslevel</param>
        public LightObjectDirectional(ShadowQuality shadowQuality)
        {
            Init(LightType.Directional, shadowQuality, SunShadowType.Default);
        }
    }
}
