using KWEngine3;
using KWEngine3.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3TestProject.Classes.WorldBloomTest
{
    internal class HUDObjectTextToast : HUDObjectText
    {
        public float TimestampStart { get; set; }
        public void Show(RenderQualityLevel l)
        {
            SetText(
                l == RenderQualityLevel.Low ? "low" :
                l == RenderQualityLevel.Default ? "default" :
                "high"
                );
            TimestampStart = KWEngine.WorldTime;
            SetOpacity(1);
        }

        public void Update()
        {
            if (KWEngine.WorldTime - TimestampStart > 1)
            {
                SetOpacity(0);
            }
        }
    }
}
