using Heirloom.Desktop;
using Heirloom.Math;

namespace Superfluid.Engine
{
    public static class Input
    {
        public static Vector MousePosition { get; private set; }

        public static void AttachToWindow(Window window)
        {
            window.MouseMove += Window_MouseMove;
        }

        private static void Window_MouseMove(Window _, MouseMoveEvent e)
        {
            MousePosition = e.Position;
        }
    }
}
