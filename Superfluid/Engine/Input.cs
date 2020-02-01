using System.Collections.Generic;

using Heirloom.Desktop;
using Heirloom.Math;

namespace Superfluid.Engine
{
    public static class Input
    {
        private static Dictionary<Key, bool> _keys = new Dictionary<Key, bool>();

        public static Vector MousePosition { get; private set; }

        public static void AttachToWindow(Window window)
        {
            window.MouseMove += Window_MouseMove;
            window.KeyRelease += Window_KeyEvent;
            window.KeyPress += Window_KeyEvent;
        }

        private static void Window_KeyEvent(Window _, KeyEvent e)
        {
            _keys[e.Key] = e.Action == ButtonAction.Press;
        }

        public static bool GetKeyDown(Key key)
        {
            if (_keys.TryGetValue(key, out var press))
            {
                return press;
            }

            return false;
        }

        private static void Window_MouseMove(Window _, MouseMoveEvent e)
        {
            MousePosition = e.Position;
        }
    }
}
