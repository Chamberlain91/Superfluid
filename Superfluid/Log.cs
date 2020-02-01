using System;
using System.Diagnostics;

namespace Superfluid
{
    public static class Log
    {
        public static void Error(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message);
            Console.ResetColor();
        }

        [Conditional("DEBUG")]
        public static void Info(string message)
        {
            Console.WriteLine(message);
        }
    }
}
