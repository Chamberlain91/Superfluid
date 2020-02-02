using System.Collections.Generic;

using Heirloom.Desktop;
using Heirloom.Math;

namespace Superfluid.Engine
{
    public static class Input
    {
        private static Dictionary<int, bool> _mouse = new Dictionary<int, bool>();
        private static Dictionary<Key, bool> _keys = new Dictionary<Key, bool>();

        public static Vector MousePosition { get; private set; }

        public static void AttachToWindow(Window window)
        {
            window.KeyRelease += Window_KeyEvent;
            window.KeyPress += Window_KeyEvent;

            window.MouseRelease += Window_MousePress;
            window.MousePress += Window_MousePress;
            window.MouseMove += Window_MouseMove;
        }

        private static void Window_MousePress(Window _, MouseButtonEvent e)
        {
            _mouse[e.Button] = e.Action == ButtonAction.Press;
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

        public static bool GetMouseDown(int button)
        {
            if (_mouse.TryGetValue(button, out var press))
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
