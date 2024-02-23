using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace KWEngine3.Helper
{
    public class FlowFieldCell
    {
        private Vector3 _worldPos;
        private Vector2i _gridIndex;

        public FlowFieldCell(Vector3 worldpos, Vector2i gridIndex)
        {
            _worldPos = worldpos;
            _gridIndex = gridIndex;
        }


    }
}
