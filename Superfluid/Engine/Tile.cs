using Heirloom.Drawing;

namespace Superfluid.Engine
{
    public sealed class Tile
    {
        public Image Image { get; }

        public TileSet TileSet { get; }

        public int Id { get; }

        public Tile(TileSet tileSet, int id, Image image)
        {
            TileSet = tileSet;
            Image = image;
            Id = id;
        }
    }
}
