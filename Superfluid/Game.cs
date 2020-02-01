using Heirloom.Collections;
using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Drawing.Extras;

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
                Map = Assets.GetMap("testmap");

                // 
                var spr = LoadPlayerSprite();
                Actor = new Player(spr);

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
                { "walk", 0.1F, Assets.GetImages("alienpink_walk1", "alienpink_walk2") },
                { "jump", 0.1F, Assets.GetImages("alienpink_jump") },
                { "idle", 5F, Assets.GetImages("alienpink_stand", "alienpink_front") },
                { "hurt", 1.0F, Assets.GetImages("alienpink_hit") }
            };

            return builder.CreateSprite();
        }

        private static void OnUpdate(Graphics gfx, float dt)
        {
            // Clear the screen
            gfx.Clear(Color.DarkGray);

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
