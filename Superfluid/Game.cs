using Heirloom.Collections;
using Heirloom.Collections.Spatial;
using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Drawing.Extras;
using Heirloom.Math;

using Superfluid.Actors;
using Superfluid.Engine;
using Superfluid.Entities;

namespace Superfluid
{
    public static class Game
    {
        public static Window Window { get; private set; }

        public static RenderLoop Loop { get; private set; }

        public static TypeDictionary<Entity> Entities { get; private set; }

        public static TileMap Map;

        public static Image Background;

        public static BoundingTreeSpatialCollection<Block> Spatial { get; private set; }

        private static void Main(string[] args)
        {
            Application.Run(() =>
            {
                // Create entities storage
                Entities = new TypeDictionary<Entity>();

                // 
                Spatial = new BoundingTreeSpatialCollection<Block>();

                // Create the game window
                Window = new Window("Superfluid!");
                Window.Graphics.EnableFPSOverlay = true;
                Window.Maximize();

                // Bind Input
                Input.AttachToWindow(Window);

                // Load game assets
                Assets.LoadDatabase();
                Assets.PackAtlas();

                // Center origins on assets prefixed by string given
                Assets.SetImagesCenterOrigin("crosshair102");
                Assets.SetImagesCenterOrigin("alien"); // alienpink_walk1, etc

                // Sets the cursor
                SetCursor("crosshair102", Color.Pink);

                // Load the background image
                Background = Assets.GetImage("colored_desert");

                // Load the test map
                LoadMap("testmap");

                // Create the player actor
                var player = new Player(LoadPlayerSprite());
                player.Transform.Position = (100, 300);

                // 
                Entities.Add(player);

                // Create main loop
                Loop = RenderLoop.Create(Window.Graphics, OnUpdate);
                Loop.Start();
            });
        }

        private static void LoadMap(string name)
        {
            Spatial.Clear();

            // Load map data (load phase)
            Map = Assets.GetMap(name);

            // Scan map data (generate phase)
            var groundLayer = Map.GetLayer("ground");
            foreach (var (x, y) in Rasterizer.Rectangle(Map.Size))
            {
                var tile = groundLayer.GetTile(x, y);
                if (tile == null) { continue; }
                else
                {
                    // Compute block position
                    var pos = new Vector(x, y) * (Vector) Map.TileSize;
                    var block = new Block((pos, Map.TileSize));

                    // 
                    Spatial.Add(block, block.Bounds);
                    Entities.Add(block);
                }
            }
        }

        private static void SetCursor(string name, Color color)
        {
            // Clone image
            var image = Assets.GetImage(name);
            var cursor = new Image(image.Size);

            // Copy pixels
            foreach (var (x, y) in Rasterizer.Rectangle(cursor.Size))
            {
                cursor.SetPixel(x, y, (Color) image.GetPixel(x, y) * color);
            }

            // Center of image
            cursor.Origin = image.Origin;
            Window.SetCursor(cursor);
        }

        public static Sprite LoadPlayerSprite()
        {
            var builder = new SpriteBuilder
            {
                { "walk", 0.1F, Assets.GetImages("alienpink_walk1_crop", "alienpink_walk2_crop") },
                { "jump", 0.1F, Assets.GetImages("alienpink_jump_crop") },
                { "idle", 5F, Assets.GetImages("alienpink_stand_crop", "alienpink_front_crop") },
                { "hurt", 1.0F, Assets.GetImages("alienpink_hit_crop") }
            };

            return builder.CreateSprite();
        }

        private static void OnUpdate(Graphics gfx, float dt)
        {
            // Clear the screen
            gfx.Clear(Color.DarkGray);

            // Draw background (skybox)
            var backgroundratio = gfx.Surface.Height / (float) (Map.Height * Map.TileSize.Height);
            gfx.DrawImage(Background, Matrix.CreateScale(backgroundratio));

            // Draw each map layer
            for (var i = 0; i < Map.LayerCount; i++)
            {
                var layer = Map.GetLayer(i);
                layer.Draw(gfx);
            }

            // Update entities
            foreach (var entity in Entities)
            {
                entity.Update(dt);
            }

            // Draw entities
            foreach (var entity in Entities)
            {
                gfx.PushState();
                entity.Draw(gfx, dt);
                entity.DebugDraw(gfx);
                gfx.PopState();
            }
        }
    }
}
