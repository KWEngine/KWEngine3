using OpenTK.Mathematics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KWEngine3.Helper
{
    internal class ScreenGridTile
    {
        public int _width;
        public int _height;
        public int _offsetX;
        public int _offsetY;
        public float _ndcLeft;
        public float _ndcRight;
        public float _ndcTop;
        public float _ndcBottom;
        public Vector2 _ndcCenter;
        public float _ndcRadius;
        public int _preparedLightsIndicesCount = 0;
        public int[] _preparedLightsIndices = new int[KWEngine.MAX_LIGHTS];


        public ScreenGridTile(int width, int height, int offsetX, int offsetY)
        {
            _width = width;
            _height = height;
            _offsetX = offsetX;
            _offsetY = offsetY;

            _ndcLeft = offsetX / (float)KWEngine.Window.Width * 2.0f - 1.0f;
            _ndcRight = (offsetX + width) / (float)KWEngine.Window.Width * 2.0f - 1.0f;
            _ndcTop = (1.0f - offsetY / (float)KWEngine.Window.Height) * 2.0f - 1.0f;
            _ndcBottom = (1.0f - ((offsetY + height) / (float)KWEngine.Window.Height)) * 2.0f - 1.0f;

            _ndcCenter = new Vector2(_ndcLeft + (_ndcRight - _ndcLeft) * 0.5f, _ndcBottom + (_ndcTop - _ndcBottom) * 0.5f);
            _ndcRadius = (new Vector2(_ndcLeft, _ndcTop) - new Vector2(_ndcRight, _ndcBottom)).Length;
        }

        public override string ToString()
        {
            return "[WxH: " + _width + "x" + _height + "] @ (" + _offsetX + "|" + _offsetY + ")";
        }
    }
}
