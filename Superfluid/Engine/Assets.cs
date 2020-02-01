using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Heirloom.Drawing;
using Heirloom.IO;
using Heirloom.Math;

namespace Superfluid.Engine
{
    public static class Assets
    {
        private const string AssetPrefix = "assets.";

        private static readonly Dictionary<string, Image> _images = new Dictionary<string, Image>();

        private static readonly Dictionary<string, TileMap> _tileMaps = new Dictionary<string, TileMap>();

        private static readonly Dictionary<string, TileSet> _tileSets = new Dictionary<string, TileSet>();

        private static readonly HashSet<string> _loaded = new HashSet<string>();

        public static IEnumerable<string> ImageNames => _images.Keys;

        public static IEnumerable<string> TileMapNames => _tileMaps.Keys;

        public static IEnumerable<string> TileSetNames => _tileSets.Keys;

        public static void LoadDatabase()
        {
            var assembly = typeof(Assets).Assembly;

            foreach (var file in Files.GetEmbeddedFiles())
            {
                if (file.Assembly == assembly)
                {
                    // Get shorest identifier
                    // should be "assets.dir.file.ext"
                    var identifier = file.Identifiers.OrderBy(d => d.Length).First();

                    // Load by identifier
                    LoadAsset(identifier);
                }
            }
        }

        public static void PackAtlas()
        {
            // Compile images into atlas
            Image.CreateAtlas(_images.Values);
        }

        public static void SetImagesCenterOrigin(params string[] prefixes)
        {
            foreach (var prefix in prefixes)
            {
                SetImagesCenterOrigin(prefix);
            }
        }

        public static void SetImagesCenterOrigin(string prefix)
        {
            var names = ImageNames.Where(name => name.StartsWith(prefix));

            foreach (var name in names)
            {
                var image = GetImage(name);
                image.Origin = 0.5F * (Vector) image.Size;
            }
        }

        public static bool LoadAsset(string identifier)
        {
            identifier = EnsureIdentifierFormat(identifier);

            // Attempt to add identifier to loaded set
            if (_loaded.Add(identifier))
            {
                // Get file extension
                var ext = Path.GetExtension(identifier);

                // Extract filename "images.pack.asset.png" to "asset"
                var name = GetAssetName(identifier);

                // Log.Debug($"{ext} -> {name}");

                using var stream = Files.OpenStream(identifier);

                switch (ext)
                {
                    case ".jpg":
                    case ".png":
                        // Load and store image
                        _images[name] = new Image(stream);
                        break;

                    case ".tmx":
                        // Load Tiled .tmx
                        _tileMaps[name] = new TileMap(stream);
                        break;

                    case ".tsx":
                        // Load Tiled .tsx
                        _tileSets[name] = new TileSet(stream);
                        break;

                    default:
                        // Didn't know what to do with this asset
                        Log.Warn($"Asset: '{identifier}' not loaded.");
                        break;
                }

                // Was a new identifier and (hopefully) loaded the asset
                return true;
            }
            else
            {
                // Not a new asset
                return false;
            }
        }

        private static string EnsureIdentifierFormat(string identifier)
        {
            // Append asset prefix if not given
            if (!identifier.StartsWith(AssetPrefix)) { identifier = AssetPrefix + identifier; }
            identifier = identifier.ToIdentifier(); // format correctly
            return identifier;
        }

        #region Get Assets

        /// <summary>
        /// Get multiple images by name.
        /// </summary>
        public static IEnumerable<Image> GetImages(params string[] names)
        {
            if (names is null) { throw new ArgumentNullException(nameof(names)); }
            if (names.Length == 0) { throw new ArgumentException("Must specify at least one name", nameof(names)); }

            // Emit each image by name
            foreach (var name in names)
            {
                yield return GetImage(name);
            }
        }

        /// <summary>
        /// Get an image by name.
        /// </summary>
        public static Image GetImage(string name)
        {
            ForbidBlank(name, nameof(name));

            // Try to get image
            if (_images.TryGetValue(name, out var image))
            {
                return image;
            }

            // Identifier not known
            throw new FileNotFoundException($"Unable to find image '{name}'.");
        }

        /// <summary>
        /// Get a tile map image by name.
        /// </summary>
        public static TileMap GetMap(string name)
        {
            ForbidBlank(name, nameof(name));

            // Try to get map
            if (_tileMaps.TryGetValue(name, out var map))
            {
                return map;
            }

            // Identifier not known
            throw new FileNotFoundException($"Unable to find tile map '{name}'.");
        }

        /// <summary>
        /// Get a tile set by name.
        /// </summary>
        public static TileSet GetTileSet(string name)
        {
            ForbidBlank(name, nameof(name));

            // Try to get tile set
            if (_tileSets.TryGetValue(name, out var set))
            {
                return set;
            }

            // Identifier not known
            throw new FileNotFoundException($"Unable to find tile set '{name}'.");
        }

        #endregion

        public static string GetAssetName(string identifier)
        {
            var name = Path.GetFileNameWithoutExtension(identifier);
            name = name.Replace(".", "/");
            name = Path.GetFileNameWithoutExtension(name);
            return name;
        }

        private static void ForbidBlank(string identifier, string name)
        {
            if (string.IsNullOrWhiteSpace(identifier))
            {
                throw new ArgumentException($"Null or empty argument {name}", name);
            }
        }
    }
}
