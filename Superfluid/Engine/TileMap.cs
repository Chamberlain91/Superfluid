using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Heirloom.IO;
using Heirloom.Math;

namespace Superfluid.Engine
{
    public class TileMap
    {
        private readonly Dictionary<int, Tile> _tileLookup = new Dictionary<int, Tile>();
        private readonly List<TileMapLayer> _layers = new List<TileMapLayer>();

        public TileMap(Stream stream)
        {
            var document = XDocument.Load(stream);

            var xMap = document.Root;

            // Extract map size
            var mapWidth = int.Parse(xMap.Attribute("width").Value);
            var mapHeight = int.Parse(xMap.Attribute("height").Value);
            Size = new IntSize(mapWidth, mapHeight);

            // Extract tile size
            var tileWidth = int.Parse(xMap.Attribute("tilewidth").Value);
            var tileHeight = int.Parse(xMap.Attribute("tileheight").Value);
            TileSize = new IntSize(tileWidth, tileHeight);

            // 
            foreach (var xElement in xMap.Elements())
            {
                // 
                switch ($"{xElement.Name}")
                {
                    case "tileset":
                    {
                        // Get tileset source attribute
                        var source = (string) xElement.Attribute("source");
                        var identifier = source.Substring(3).ToIdentifier(); // trim '../'
                        var name = Assets.GetAssetName(identifier);

                        // 
                        var offset = int.Parse((string) xElement.Attribute("firstgid"));

                        // Load and get asset
                        Assets.LoadAsset(identifier);
                        var tileSet = Assets.GetTileSet(name);

                        // Store remapped tiles
                        foreach (var id in tileSet.Ids)
                        {
                            _tileLookup[offset + id] = tileSet.GetTile(id);
                        }
                    }
                    break;

                    case "layer":
                    {
                        var name = (string) xElement.Attribute("name");

                        var xData = xElement.Element("data");
                        var csv = xData.Value;

                        var tiles = new Tile[mapHeight, mapWidth];
                        var cells = csv.Trim().Split(",");
                        for (var y = 0; y < mapHeight; y++)
                        {
                            for (var x = 0; x < mapWidth; x++)
                            {
                                var id = int.Parse(cells[x + (y * mapWidth)]);
                                if (id == 0) { continue; } // no defined tile

                                tiles[y, x] = _tileLookup[id];
                            }
                        }

                        // Append layer
                        _layers.Add(new TileMapLayer(this, name, mapWidth, mapHeight, tiles));
                    }
                    break;
                }
            }
        }

        public int Width => Size.Width;

        public int Height => Size.Height;

        public IntSize TileSize { get; }

        public IntSize Size { get; }

        public IEnumerable<string> LayerNames => _layers.Select(l => l.Name);

        public int LayerCount => _layers.Count;

        public TileMapLayer GetLayer(string name)
        {
            return _layers.First(l => l.Name == name);
        }

        public TileMapLayer GetLayer(int index)
        {
            return _layers[index];
        }
    }
}
