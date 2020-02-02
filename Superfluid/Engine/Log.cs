using System;
using System.Diagnostics;

namespace Superfluid.Engine
{
    public static class Log
    {
        public static void Warn(object message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Error(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        [Conditional("DEBUG")]
        public static void Debug(object message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        public static void Info(object message)
        {
            Console.WriteLine(message);
        }
    }
}
