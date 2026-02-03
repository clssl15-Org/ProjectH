using System;
using System.Threading;

namespace BlackboxSystem
{
    public enum LogLevel
    {
        Normal,
        Warning,
    }

    internal static class Infrastructure
    {
        // Front
        public static string LogDirectory { get; set; }
        public static Action<string> NormalLogger { get; set; }
        public static Action<string> WarningLogger { get; set; }

        public static int MaxLogCount { get; set; } = 100;
        public static bool StrongReference { get; set; } = false;
        public static int DefaultRecursionDepth { get; set; } = 100;

        public static bool IsPrinted => Volatile.Read(ref _isPrinted) != 0;
        private static int _isPrinted = 0;

        public static ExportFormat ExportFormat { get; set; } = ExportFormat.Html;
        public static FullExportOption FullExportOption { get; set; } = FullExportOption.Full;
        public static OpenLogOption OpenLogOption { get; set; } = OpenLogOption.Open;


        // Content
        public static bool TryMarkPrinted() => Interlocked.CompareExchange(ref _isPrinted, 1, 0) == 0;
        public static void ForceResetRuntimeState() => Volatile.Write(ref _isPrinted, 0);

        public static bool Log(string message, LogLevel logLevel = LogLevel.Normal)
        {
            var logger = logLevel switch
            {
                LogLevel.Normal => NormalLogger,
                LogLevel.Warning => WarningLogger,
                _ => null,
            };

            if (logger == null)
                return false;

            logger(message);
            return true;
        }
    }
}
