namespace KWEngine3.Helper
{
    internal class ScreenGrid
    {
        public int _tileSize = 128;
        public ScreenGridTile[] _tiles;
        public int _width;
        public int _height;

        public ScreenGrid(int width, int height)
        {
            _tileSize = width / 8;

            _width = width;
            _height = height;
            List<ScreenGridTile> _tileList = new();

            int numberOfTilesX = (int)Math.Ceiling((float)_width / _tileSize);
            int numberOfTilesY = (int)Math.Ceiling((float)_height / _tileSize);
            int numberOfTilesTotal = numberOfTilesX * numberOfTilesY;

            int offsetY = 0;
            int offsetX = 0;
            for(int i = 0; i < numberOfTilesTotal; i++)
            {
                ScreenGridTile tile;
                if (offsetX + _tileSize < _width)
                {
                    if(offsetY + _tileSize < height)
                    {
                        tile = new ScreenGridTile(_tileSize, _tileSize, offsetX, offsetY);
                    }
                    else
                    {
                        tile = new ScreenGridTile(_tileSize, height - offsetY, offsetX, offsetY);
                    }
                    offsetX += _tileSize;
                }
                else
                {
                    if (offsetY + _tileSize < height)
                    {
                        tile = new ScreenGridTile(width - offsetX, _tileSize, offsetX, offsetY);
                    }
                    else
                    {
                        tile = new ScreenGridTile(width - offsetX, height - offsetY, offsetX, offsetY);
                    }
                    offsetY += _tileSize;
                    offsetX = 0;
                }
                _tileList.Add(tile);
            }
            _tiles = _tileList.ToArray();
        }
    }
}
