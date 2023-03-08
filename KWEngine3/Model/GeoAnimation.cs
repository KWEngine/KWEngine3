using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Model
{
    /// <summary>
    /// Animationsklasse
    /// </summary>
    public struct GeoAnimation
    {
        /// <summary>
        /// Name der Animation
        /// </summary>
        public string Name { get; internal set; }
        /// <summary>
        /// Dauer der Animation
        /// </summary>
        public float DurationInTicks { get; internal set; }
        /// <summary>
        /// Ticks pro Sekunde
        /// </summary>
        public float TicksPerSecond { get; internal set; }
        /// <summary>
        /// Animationskanäle
        /// </summary>
        internal Dictionary<string, GeoNodeAnimationChannel> AnimationChannels { get; set; }
    }
}
