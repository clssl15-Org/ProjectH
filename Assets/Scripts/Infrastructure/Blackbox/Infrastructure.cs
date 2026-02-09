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

        public static ExportFormat ExportFormat
        {
            get => _exportFormat;
            set
            {
                if (value != ExportFormat.BasedOnSettings)
                    _exportFormat = value;
            }
        }
        public static FullExportOption FullExportOption
        {
            get => _fullExportOption;
            set
            {
                if (value != FullExportOption.BasedOnSettings)
                    _fullExportOption = value;
            }
        }
        public static OpenLogOption OpenLogOption
        {
            get => _openLogOption;
            set
            {
                if (value != OpenLogOption.BasedOnSettings)
                    _openLogOption = value;
            }
        }
        public static ExceptionHandlingOption ExceptionHandlingOption
        {
            get => _exceptionHandlingOption;
            set
            {
                if (value != ExceptionHandlingOption.BasedOnSettings)
                    _exceptionHandlingOption = value;
            }
        }

        private static ExportFormat _exportFormat = ExportFormat.Html;
        private static FullExportOption _fullExportOption = FullExportOption.Full;
        private static OpenLogOption _openLogOption = OpenLogOption.Open;
        private static ExceptionHandlingOption _exceptionHandlingOption = ExceptionHandlingOption.None;


        // Content
        public static bool TryMarkPrinted() => Interlocked.CompareExchange(ref _isPrinted, 1, 0) == 0;
        public static void ForceResetRuntimeState() => Volatile.Write(ref _isPrinted, 0);

        public static bool Log(string message, LogLevel logLevel = LogLevel.Normal)
        {
            var logger = logLevel switch
            {
                LogLevel.Normal => NormalLogger,
                LogLevel.Warning => WarningLogger,
                _ => NormalLogger,
            };

            if (logger == null)
                return false;

            logger(message);
            return true;
        }

        public static ExportFormat Resolve(this ExportFormat format) => format == ExportFormat.BasedOnSettings ? ExportFormat : format;
        public static FullExportOption Resolve(this FullExportOption option) => option == FullExportOption.BasedOnSettings ? FullExportOption : option;
        public static OpenLogOption Resolve(this OpenLogOption option) => option == OpenLogOption.BasedOnSettings ? OpenLogOption : option;
        public static ExceptionHandlingOption Resolve(this ExceptionHandlingOption option) => option == ExceptionHandlingOption.BasedOnSettings ? ExceptionHandlingOption : option;
    }
}
