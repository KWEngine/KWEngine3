using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Model
{
    internal class GeoNodeAnimationChannel
    {
        public List<GeoAnimationKeyframe> ScaleKeys { get; internal set; }
        public List<GeoAnimationKeyframe> RotationKeys { get; internal set; }
        public List<GeoAnimationKeyframe> TranslationKeys { get; internal set; }
        public string NodeName { get; internal set; } = null;

        // Bitmask pro Keyframe-Index: welche Achsen/Komponenten disabled sind.
        // PositionX/Y/Z-Bits für Translation, je 1 für Rotation und Scale (0 = aktiv, != 0 = disabled).
        // null = keine Keyframes disabled (Normalfall).
        internal int[] DisabledTranslationKeyAxes = null;
        internal int[] DisabledRotationKeyAxes = null;
        internal int[] DisabledScaleKeyAxes = null;
    }
}
