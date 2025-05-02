using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.GameObjects
{
    /// <summary>
    /// Klasse für das Sonnenlicht
    /// </summary>
    public class LightObjectSun : LightObject
    {
        /// <summary>
        /// Konstruktormethode für die Erstellung einer Sonnenlichtinstanz
        /// </summary>
        /// <param name="shadowQuality">Schattenqualitätslevel</param>
        /// <param name="shadowType">Gibt die Art des Schattens an</param>
        public LightObjectSun(ShadowQuality shadowQuality, SunShadowType shadowType)
        {
            Init(LightType.Sun, shadowQuality, shadowType);
        }


        /// <summary>
        /// Gibt an, wie viel größer die äußere Shadow Map für eine schattenwerfende Sonne ist
        /// </summary>
        /// <param name="factor">Vergrößerungsfaktor</param>
        public void SetCSMFactor(CSMFactor factor)
        {
            _csmFactor = factor;           
        }


        internal CSMFactor _csmFactor = CSMFactor.Four;
    }
}
