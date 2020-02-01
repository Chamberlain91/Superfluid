using System;
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
                Assets.LoadAssetDatabase();

                // Create main loop
                Loop = RenderLoop.Create(Window.Graphics, OnUpdate);
                Loop.Start();
            });
        }

        private static void OnUpdate(Graphics gfx, float dt)
        {
            gfx.Clear(Color.DarkGray);
            gfx.DrawImage(Assets.GetImage("blue_desert"), (0, 0));
        }
    }
}
