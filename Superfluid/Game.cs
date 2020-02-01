using Heirloom.Collections;
using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Math;
using Superfluid.Engine;

namespace Superfluid
{
    public static class Game
    {
        public static Window Window { get; private set; }

        public static RenderLoop Loop { get; private set; }

        public static TypeDictionary<Entity> Entities { get; private set; }

        static public TileMap Map;

        private static void Main(string[] args)
        {
            Application.Run(() =>
            {
                // Create entities storage
                Entities = new TypeDictionary<Entity>();

                // Create the game window
                Window = new Window("Superfluid!");
                Window.Maximize();

                // Load game assets
                Assets.LoadDatabase();
                Assets.PackAtlas();

                // 
                Map = Assets.GetMap("testmap");

                // Create main loop
                Loop = RenderLoop.Create(Window.Graphics, OnUpdate);
                Loop.Start();
            });
        }

        private static void OnUpdate(Graphics gfx, float dt)
        {
            // 
            gfx.Clear(Color.DarkGray);

            // Draw each map layer
            for (var i = 0; i < Map.LayerCount; i++)
            {
                DrawMapLayer(gfx, Map.GetLayer(i));
            }
        }

        private static void DrawMapLayer(Graphics gfx, TileMapLayer layer)
        {
            for (var y = 0; y < Map.Height; y++)
            {
                for (var x = 0; x < Map.Height; x++)
                {
                    var tile = layer.GetTile(x, y);
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
