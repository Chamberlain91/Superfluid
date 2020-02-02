using Heirloom.Drawing;
using Heirloom.Math;

namespace Superfluid.Engine
{
    public class TileMapLayer
    {
        private readonly Tile[,] _tiles;

        public TileMapLayer(TileMap map, string name, int width, int height, Tile[,] tiles)
        {
            Map = map;
            Name = name;

            Width = width;
            Height = height;

            _tiles = tiles;
        }

        public TileMap Map { get; }

        public string Name { get; }

        public int Width { get; }

        public int Height { get; }

        public Tile GetTile(int x, int y)
        {
            return _tiles[y, x];
        }

        public void Draw(Graphics gfx)
        {
            for (var y = 0; y < Map.Height; y++)
            {
                for (var x = 0; x < Map.Width; x++)
                {
                    var tile = GetTile(x, y);
                    if (tile == null) { continue; }

                    // Compute tile position
                    var co = new IntVector(x, y);
                    var pos = co * (IntVector) Map.TileSize;
                    pos.Y += Map.TileSize.Height - tile.Image.Height;

                    // Draw tile
                    gfx.DrawImage(tile.Image, pos);
                }
            }
        }
    }
}
