using System;

namespace BlackboxSystem
{
    internal static class Infrastructure
    {
        public static string LogDirectory { get; set; }
        public static Action<string> Logger { get; set; }

        public static int MaxLogCount { get; set; } = 100;
        public static bool StrongReference { get; set; } = false;

        public static bool Log(string message)
        {
            if (Logger == null)
                return false;

            Logger(message);
            return true;
        }
    }
}
