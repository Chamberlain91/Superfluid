using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using Heirloom.IO;

namespace Superfluid.Engine
{
    public class TileSet
    {
        private readonly Dictionary<int, Tile> _tiles = new Dictionary<int, Tile>();

        public TileSet(Stream stream)
        {
            var document = XDocument.Load(stream);

            var xTileset = document.Root;

            foreach (var xTile in xTileset.Elements("tile"))
            {
                var id = int.Parse((string) xTile.Attribute("id"));

                // Get tileset source attribute
                var source = (string) xTile.Element("image").Attribute("source");
                var identifier = source.Substring(3).ToIdentifier(); // trim '../'
                var name = Assets.GetAssetName(identifier);

                // Ensure image is loaded...
                Assets.LoadAsset(identifier);

                // Store tile
                _tiles[id] = new Tile(this, id, Assets.GetImage(name));
            }
        }

        public IEnumerable<int> Ids => _tiles.Keys;

        public Tile GetTile(int index)
        {
            return _tiles[index];
        }
    }
}
