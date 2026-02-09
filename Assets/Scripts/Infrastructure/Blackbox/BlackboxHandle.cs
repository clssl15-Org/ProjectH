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

    public enum ExceptionHandlingOption
    {
        None,
        CrashExport,
        BasedOnSettings,
    }

    public struct BlackboxHandle
    {
        // Forwarders
        public object Owner => Blackbox?.Owner;
        public string OwnerString => Blackbox?.OwnerString ?? InvalidHandleMessage;
        public long Id => Blackbox?.Id ?? -1;
        public bool IsValid => Blackbox != null;

        private const string InvalidHandleMessage = "[BlackboxHandle] Invalid Handle";

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
        public static int DefaultRecursionDepth
        {
            get => Infrastructure.DefaultRecursionDepth;
            set => Infrastructure.DefaultRecursionDepth = value;
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
        private Blackbox Blackbox
        {
            get
            {
                if (_subject == null) return null;
                return _blackbox ??= BlackboxRegistry.GetBlackbox(_subject);
            }
        }
        private Blackbox _blackbox;
        private object _subject;


        // Content
        private BlackboxHandle(object subject)
        {
            _blackbox = null;
            _subject = subject;
        }

        #region Configuration
        public static void Configure(
            string logDirectory,
            Action<string> logger,
            bool strongReference = false,
            ExportFormat exportFormat = ExportFormat.BasedOnSettings,
            FullExportOption fullExportOption = FullExportOption.BasedOnSettings,
            OpenLogOption openLogOption = OpenLogOption.BasedOnSettings,
            ExceptionHandlingOption exceptionHandlingOption = ExceptionHandlingOption.BasedOnSettings)
        {
            Configure(logDirectory, logger, logger, strongReference, exportFormat, fullExportOption, openLogOption, exceptionHandlingOption);
        }

        public static void Configure(
            string logDirectory,
            Action<string> normalLogger,
            Action<string> warningLogger,
            bool strongReference = false,
            ExportFormat exportFormat = ExportFormat.BasedOnSettings,
            FullExportOption fullExportOption = FullExportOption.BasedOnSettings,
            OpenLogOption openLogOption = OpenLogOption.BasedOnSettings,
            ExceptionHandlingOption exceptionHandlingOption = ExceptionHandlingOption.BasedOnSettings)
        {
            Infrastructure.LogDirectory = logDirectory;
            Infrastructure.NormalLogger = normalLogger;
            Infrastructure.WarningLogger = warningLogger;
            Infrastructure.StrongReference = strongReference;

            Infrastructure.ExportFormat = exportFormat;
            Infrastructure.FullExportOption = fullExportOption;
            Infrastructure.OpenLogOption = openLogOption;
            Infrastructure.ExceptionHandlingOption = exceptionHandlingOption;
        }
        public static void ForceReset() => BlackboxRegistry.ForceReset();
        #endregion

        public static BlackboxHandle Of<T>(T subject) where T : class
        {
#if !BLACKBOX
            return default;
#else
            if (subject == null)
                throw new ArgumentNullException(
                    nameof(subject),
                    $"{nameof(subject)} cannot be null.");

            return new BlackboxHandle(subject);
#endif
        }

        public readonly BlackboxHandle When(bool condition)
        {
#if !BLACKBOX
            return default;
#else
            return condition ? this : default;
#endif
        }
        public BlackboxHandle When(Func<bool> predicate)
        {
#if !BLACKBOX
            return default;
#else
            if (predicate == null)
                throw new ArgumentNullException(
                    nameof(predicate),
                    FormatLogMessage($"{nameof(predicate)} cannot be null."));

            return predicate() ? this : default;
#endif
        }

        public string Write(object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = ToMessageString(message);
            return Blackbox?.Write(messageStr, methodName) ?? messageStr;
        }
        public DisposableHandle WriteScope(object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = ToMessageString(message);
            return Blackbox?.WriteScope(messageStr, methodName) ?? default;
        }
        public string Exert(object other, object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = ToMessageString(message);
            return Blackbox?.Exert(BlackboxRegistry.GetBlackbox(other), messageStr, methodName) ?? messageStr;
        }
        public string Exerted(object other, object message, [CallerMemberName] string methodName = "")
        {
            if (Blackbox == null) return ToMessageString(message);
            return BlackboxHandle.Of(other).Exert(Owner, message, methodName);
        }
        public DisposableHandle ExertScope(object other, object message, [CallerMemberName] string methodName = "")
        {
            return Blackbox?.ExertScope(BlackboxRegistry.GetBlackbox(other), ToMessageString(message), methodName) ?? default;
        }
        public DisposableHandle ExertedScope(object other, object message, [CallerMemberName] string methodName = "")
        {
            return Blackbox?.ExertedScope(BlackboxRegistry.GetBlackbox(other), ToMessageString(message), methodName) ?? default;
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


        public readonly struct ErrorContainer
        {
            internal readonly (string context, object target)[] Targets;

            public ErrorContainer(params (string context, object target)[] targets) => Targets = targets;
            public static implicit operator ErrorContainer((string context, object target) target) => new ErrorContainer(target);
        }
        public string WriteError(object message, ErrorContainer? others = null, ExceptionHandlingOption exceptionHandlingOption = ExceptionHandlingOption.BasedOnSettings, [CallerMemberName] string methodName = "")
        {
            var messageStr = ToMessageString(message);
            
            if (exceptionHandlingOption.Resolve() == ExceptionHandlingOption.CrashExport)
            {
                CrashExport(messageStr, others, methodName: methodName);
                return messageStr;
            }

            WriteErrorToBlackbox(messageStr, others, methodName);
            return messageStr;
        }
        public string CrashExport(object message, ErrorContainer? others = null, int? recursionDepth = null, ExportFormat format = ExportFormat.Html, FullExportOption fullExport = FullExportOption.BasedOnSettings, OpenLogOption openLog = OpenLogOption.BasedOnSettings, [CallerMemberName] string methodName = "")
        {
            var messageStr = ToMessageString(message);

            Infrastructure.Log($"[Blackbox] CRASH: {messageStr}", LogLevel.Warning);
            WriteErrorToBlackbox(messageStr, others, methodName);
            Write($"[STACK TRACE]\n{new StackTrace(true)}\n");

            ExportInternal(recursionDepth ?? DefaultRecursionDepth, true, format, fullExport, openLog);
            return messageStr;
        }
        private void WriteErrorToBlackbox(string message, ErrorContainer? others, string methodName)
        {
            Blackbox?.Write($"[Error] {message}", methodName);

            if (others.HasValue && others.Value.Targets != null)
                foreach (var (context, target) in others.Value.Targets)
                {
                    if (target == null)
                    {
                        Blackbox?.Write($"[Error: {context} (null)] {message}", methodName);
                        continue;
                    }

                    Blackbox?.Exert(BlackboxRegistry.GetBlackbox(target), $"[Error: {context}] {message}", methodName);
                }
        }

        public void Export(int? recursionDepth = null, ExportFormat format = ExportFormat.BasedOnSettings, FullExportOption fullExportOption = FullExportOption.BasedOnSettings, OpenLogOption openLogOption = OpenLogOption.BasedOnSettings) =>
            ExportInternal(recursionDepth ?? DefaultRecursionDepth, false, format, fullExportOption, openLogOption);

        private void ExportInternal(int recursionDepth, bool isCrash, ExportFormat format, FullExportOption fullExportOption, OpenLogOption openLogOption)
        {
            if (Blackbox == null)
                return;

            if (!Infrastructure.TryMarkPrinted())
            {
                Infrastructure.Log(FormatLogMessage(
                    $"Blackbox has already been exported. Skipping duplicate export."),
                    LogLevel.Warning);
                return;
            }

            var fullExport = fullExportOption.Resolve() switch
            {
                FullExportOption.Full => true,
                FullExportOption.Focused => false,
                _ => false,
            };
            var openLog = openLogOption.Resolve() switch
            {
                OpenLogOption.Open => true,
                OpenLogOption.Never => false,
                _ => false,
            };

            switch (format.Resolve())
            {
                case ExportFormat.Html:
                    HtmlExporter.Export(Blackbox, recursionDepth, isCrash, fullExport, openLog);
                    break;

                default:
                    TxtExporter.Export(Blackbox, recursionDepth, isCrash, fullExport, openLog);
                    break;
            }
        }

        private readonly string ToMessageString(object obj) => obj?.ToString() ?? "null";

        private string FormatLogMessage(string message)
        {
            var nametag = "BlackboxHandle";
            if (Blackbox != null) nametag += $": {Blackbox.OwnerString}";

            return $"[{nametag}] {message}";
        }
    }
}
