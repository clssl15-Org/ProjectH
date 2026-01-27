using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BlackboxSystem.Exporters;

namespace BlackboxSystem
{
    public enum ExportFormat
    {
        Txt,
        Html,
        BasedOnSettings,
    }

    public enum FullExportOption
    {
        Focused,
        Full,
        BasedOnSettings,
    }

    public enum OpenLogOption
    {
        Open,
        Never,
        BasedOnSettings,
    }

    public readonly struct BlackboxHandle
    {
        // Forwarders
        public object Owner { get { ThrowIfInvalid(); return _blackbox.Owner; } }
        public string OwnerString { get { ThrowIfInvalid(); return _blackbox.OwnerString; } }
        public long Id { get { ThrowIfInvalid(); return _blackbox.Id; } }

        public static string LogDirectory
        {
            get => Infrastructure.LogDirectory;
            set => Infrastructure.LogDirectory = value;
        }
        public static Action<string> NormalLogger
        {
            get => Infrastructure.NormalLogger;
            set => Infrastructure.NormalLogger = value;
        }
        public static Action<string> WarningLogger
        {
            get => Infrastructure.WarningLogger;
            set => Infrastructure.WarningLogger = value;
        }
        public static int MaxLogCount
        {
            get => Infrastructure.MaxLogCount;
            set => Infrastructure.MaxLogCount = value;
        }
        public static bool StrongReference
        {
            get => Infrastructure.StrongReference;
            set => Infrastructure.StrongReference = value;
        }

        public static ExportFormat ExportFormat
        {
            get => Infrastructure.ExportFormat;
            set => Infrastructure.ExportFormat = value;
        }
        public static FullExportOption FullExportOption
        {
            get => Infrastructure.FullExportOption;
            set => Infrastructure.FullExportOption = value;
        }
        public static OpenLogOption OpenLogOption
        {
            get => Infrastructure.OpenLogOption;
            set => Infrastructure.OpenLogOption = value;
        }

        // Internal
        private readonly Blackbox _blackbox;


        // Content
        internal BlackboxHandle(Blackbox blackbox) => _blackbox = blackbox;

        #region Static Methods
        public static void Initialize(Action<string> logger, bool strongReference = false) => Initialize(string.Empty, logger, logger, strongReference);
        public static void Initialize(string logDirectory, Action<string> logger, bool strongReference = false) => Initialize(logDirectory, logger, logger, strongReference);
        public static void Initialize(string logDirectory, Action<string> normalLogger, Action<string> warningLogger, bool strongReference = false)
        {
            Infrastructure.LogDirectory = logDirectory;
            Infrastructure.NormalLogger = normalLogger;
            Infrastructure.WarningLogger = warningLogger;
            Infrastructure.StrongReference = strongReference;
        }
        public static void ForceReset() => BlackboxRegistry.ForceReset();
        #endregion

        public static BlackboxHandle Of(object subject)
        {
#if !BLACKBOX
            return default;
#else
            if (subject == null) throw new ArgumentNullException(nameof(subject));
            return new BlackboxHandle(BlackboxRegistry.GetBlackbox(subject));
#endif
        }

        public string Write(object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = ToMessageString(message);
            return _blackbox?.Write(messageStr, methodName) ?? messageStr;
        }
        public DisposableHandle WriteScope(object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = ToMessageString(message);
            return _blackbox?.WriteScope(messageStr, methodName) ?? default;
        }
        public string Exert(object other, object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = ToMessageString(message);
            return _blackbox?.Exert(BlackboxRegistry.GetBlackbox(other), messageStr, methodName) ?? messageStr;
        }
        public string Exerted(object other, object message, [CallerMemberName] string methodName = "")
        {
            if (_blackbox == null) return ToMessageString(message);
            return BlackboxHandle.Of(other).Exert(Owner, message, methodName);
        }
        public DisposableHandle ExertScope(object other, object message, [CallerMemberName] string methodName = "")
        {
            return _blackbox?.ExertScope(BlackboxRegistry.GetBlackbox(other), ToMessageString(message), methodName) ?? default;
        }
        public DisposableHandle ExertedScope(object other, object message, [CallerMemberName] string methodName = "")
        {
            return _blackbox?.ExertedScope(BlackboxRegistry.GetBlackbox(other), ToMessageString(message), methodName) ?? default;
        }

        public string WriteOrExerted(object message, object other, [CallerMemberName] string methodName = "")
        {
            if (other == null) return Write(message, methodName); 
            return Exerted(other, message, methodName);
        }
        public DisposableHandle WriteOrExertedScope(object message, object other, [CallerMemberName] string methodName = "")
        {
            if (other == null) return WriteScope(message, methodName);
            return ExertedScope(other, message, methodName);
        }

        public string CrashExport(string message, int recursionDepth = -1, ExportFormat format = ExportFormat.Html, FullExportOption fullExport = FullExportOption.BasedOnSettings, OpenLogOption openLog = OpenLogOption.BasedOnSettings)
        {
            Infrastructure.Log($"[BlackboxHandle] CRASH: {message}", LogLevel.Warning);
            Write($"[CRASH] {message}");
            Write($"[STACK TRACE]\n{new StackTrace(true)}\n");

            ExportInternal(recursionDepth, true, format, fullExport, openLog);
            return message;
        }

        public void Export(int recursionDepth = -1, ExportFormat format = ExportFormat.BasedOnSettings, FullExportOption fullExportOption = FullExportOption.BasedOnSettings, OpenLogOption openLogOption = OpenLogOption.BasedOnSettings) =>
            ExportInternal(recursionDepth, false, format, fullExportOption, openLogOption);

        private void ExportInternal(int recursionDepth, bool isCrash, ExportFormat format, FullExportOption fullExportOption, OpenLogOption openLogOption)
        {
            if (_blackbox == null)
                return;

            if (!Infrastructure.TryMarkPrinted())
            {
                Infrastructure.Log(
                    $"[Blackbox] Blackbox has already been exported. Skipping duplicate export.",
                    LogLevel.Warning);
                return;
            }

            var fullExport = fullExportOption switch
            {
                FullExportOption.Full => true,
                FullExportOption.Focused => false,
                FullExportOption.BasedOnSettings => Infrastructure.FullExportOption == FullExportOption.Full,
                _ => false,
            };
            var openLog = openLogOption switch
            {
                OpenLogOption.Open => true,
                OpenLogOption.Never => false,
                OpenLogOption.BasedOnSettings => Infrastructure.OpenLogOption == OpenLogOption.Open,
                _ => false,
            };

            if (format == ExportFormat.BasedOnSettings)
                format = Infrastructure.ExportFormat;

            switch (format)
            {
                case ExportFormat.Html:
                    HtmlExporter.Export(_blackbox, recursionDepth, isCrash, fullExport, openLog);
                    break;

                default:
                    TxtExporter.Export(_blackbox, recursionDepth, isCrash, fullExport, openLog);
                    break;
            }
        }

        private string ToMessageString(object obj) => obj?.ToString() ?? "null";

        private void ThrowIfInvalid()
        {
            if (_blackbox == null)
                throw new InvalidOperationException("[BlackboxHandle] Invalid handle.");
        }
    }
}
