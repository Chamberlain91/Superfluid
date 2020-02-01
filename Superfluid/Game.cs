using Heirloom.Collections;
using Heirloom.Desktop;
using Heirloom.Drawing;

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
            // Clear the screen
            gfx.Clear(Color.DarkGray);

            // Draw each map layer
            for (var i = 0; i < Map.LayerCount; i++)
            {
                var layer = Map.GetLayer(i);
                layer.Draw(gfx);
            }
        }
    }
}
