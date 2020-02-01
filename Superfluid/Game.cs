using Heirloom.Collections;
using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Drawing.Extras;
using Heirloom.Math;
using Superfluid.Actors;
using Superfluid.Engine;

namespace Superfluid
{
    public static class Game
    {
        public static Window Window { get; private set; }

        public static RenderLoop Loop { get; private set; }

        public static TypeDictionary<Entity> Entities { get; private set; }

        public static TileMap Map;

        public static Actor Actor;

        public static Image Background;

        private static void Main(string[] args)
        {
            Application.Run(() =>
            {
                // Create entities storage
                Entities = new TypeDictionary<Entity>();

                // Create the game window
                Window = new Window("Superfluid!");
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

                // 
                Background = Assets.GetImage("colored_desert");

                // 
                Map = Assets.GetMap("testmap");

                // 
                Actor = new Player(LoadPlayerSprite());
                Actor.Transform.Position = (100, 300);

                // Create main loop
                Loop = RenderLoop.Create(Window.Graphics, OnUpdate);
                Loop.Start();
            });
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

            // 
            var backgroundratio = gfx.Surface.Height / (float) (Map.Height * Map.TileSize.Height);
            gfx.DrawImage(Background, Matrix.CreateScale(backgroundratio));

            // Draw each map layer
            for (var i = 0; i < Map.LayerCount; i++)
            {
                var layer = Map.GetLayer(i);
                layer.Draw(gfx);
            }

            // Update Actor
            Actor.Update(dt);

            // Draw Actor
            gfx.PushState();
            Actor.Draw(gfx, dt);
            gfx.PopState();
        }
    }
}
