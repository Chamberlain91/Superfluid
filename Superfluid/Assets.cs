using System;
using System.Collections.Generic;
using System.IO;

using Heirloom.Drawing;
using Heirloom.IO;

namespace Superfluid
{
    public static class Assets
    {
        private const string AssetPrefix = "superfluid.assets.";

        private static readonly Dictionary<string, Image> _images = new Dictionary<string, Image>();

        public static void LoadAssetDatabase()
        {
            foreach (var file in Files.GetEmbeddedFiles())
            {
                using var stream = file.OpenStream();

                var identifier = file.Identifiers[0];

                // Get file extension
                var ext = Path.GetExtension(identifier);

                // Get file name
                if (identifier.StartsWith(AssetPrefix))
                {
                    var name = Path.GetFileNameWithoutExtension(identifier);
                    name = name.Replace(".", "/");
                    name = Path.GetFileNameWithoutExtension(name);

                    // If a PNG
                    if (ext == ".png")
                    {
                        // 
                        Log.Debug(name);

                        // Load and store image
                        _images[name] = new Image(stream);
                    }
                }
            }

            // Compile images into atlas
            Image.CreateAtlas(_images.Values);
        }

        /// <summary>
        /// Get an image by name.
        /// </summary>
        public static Image GetImage(string identifier)
        {
            const string ParamName = nameof(identifier);
            ForbidBlank(identifier, ParamName);

            // Try to get image
            if (_images.TryGetValue(identifier, out var image))
            {
                return image;
            }

            // Identifier not known
            throw new FileNotFoundException($"Unable to find image '{identifier}'.");
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
