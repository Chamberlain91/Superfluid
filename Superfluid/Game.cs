using System.Collections.Generic;

using Heirloom.Desktop;
using Heirloom.Drawing;
using Heirloom.Sound.Effects;
using Heirloom.Sound;

namespace Superfluid
{
    public static class Game
    {
        public static Window Window { get; private set; }

        public static RenderLoop Loop { get; private set; }

        private static void Main(string[] args)
        {
            Application.Run(() =>
            {
                // Create the game window
                Window = new Window("Superfluid!");
                Window.Maximize();

                // Create main loop
                Loop = RenderLoop.Create(Window.Graphics, OnUpdate);
                Loop.Start();
            });
        }

        private static void OnUpdate(Graphics gfx, float dt)
        {
            gfx.Clear(Color.DarkGray);
        }
    }
}
