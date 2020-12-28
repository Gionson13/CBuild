using System;

namespace CBuild.Core
{
    public static class Log
    {
        public static void Error(string message, string origin, string additionalInfo)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("ERROR ");
            Console.ResetColor();
            Console.WriteLine($"{origin} -> {message}");
            if (!string.IsNullOrWhiteSpace(additionalInfo))
                Console.WriteLine(additionalInfo);
        }

        public static void Warning(string message, string origin, string additionalInfo)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("WARNING ");
            Console.ResetColor();
            Console.WriteLine($"{origin} -> {message}");
            if (!string.IsNullOrWhiteSpace(additionalInfo))
                Console.WriteLine(additionalInfo);
        }
    }
}