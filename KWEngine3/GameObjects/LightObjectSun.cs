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
        /// <param name="shadowType">Gibt die Art des Schattens an (Standard oder Cascaded)</param>
        public LightObjectSun(ShadowQuality shadowQuality, ShadowType shadowType = ShadowType.Default)
        {
            Init(LightType.Sun, shadowQuality, ShadowType);
        }
    }
}
