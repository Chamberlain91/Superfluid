namespace Superfluid.Engine
{
    public class TileMapLayer
    {
        private readonly Tile[,] _tiles;

        public TileMapLayer(string name, int width, int height, Tile[,] tiles)
        {
            Name = name;

            Width = width;
            Height = height;

            _tiles = tiles;
        }

        public string Name { get; }

        public int Width { get; }

        public int Height { get; }

        public Tile GetTile(int x, int y)
        {
            return _tiles[y, x];
        }
    }
}
