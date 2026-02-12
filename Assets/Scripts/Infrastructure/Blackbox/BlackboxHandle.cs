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

    public partial struct BlackboxHandle
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

        [Conditional("BLACKBOX")]
        public void Write([InterpolatedStringHandlerArgument("")] ref WriteHandler handler, [CallerMemberName] string methodName = "")
        {
            if (!handler.ShouldLog) return;
            WriteMessage(handler.GetTextAndClear(), methodName);
        }
        [Conditional("BLACKBOX")]
        public void Write(string message, [CallerMemberName] string methodName = "")
        {
            WriteMessage(message, methodName);
        }
        public string WriteMessage(string message, [CallerMemberName] string methodName = "")
        {
            return Blackbox?.Write(message, methodName) ?? message;
        }

        public DisposableHandle WriteScope([InterpolatedStringHandlerArgument("")] ref WriteHandler handler, [CallerMemberName] string methodName = "")
        {
            if (!handler.ShouldLog) return default;
            return WriteScope(handler.GetTextAndClear(), methodName);
        }
        public DisposableHandle WriteScope(string message, [CallerMemberName] string methodName = "")
        {
            return Blackbox?.WriteScope(message, methodName) ?? default;
        }

        [Conditional("BLACKBOX")]
        public void Exert<T>(T other, [InterpolatedStringHandlerArgument("")] ref WriteHandler handler, [CallerMemberName] string methodName = "") where T : class
        {
            if (!handler.ShouldLog) return;
            ExertMessage(other, handler.GetTextAndClear(), methodName);
        }
        [Conditional("BLACKBOX")]
        public void Exert<T>(T other, string message, [CallerMemberName] string methodName = "") where T : class
        {
            ExertMessage(other, message, methodName);
        }
        public string ExertMessage<T>(T other, string message, [CallerMemberName] string methodName = "") where T : class
        {
            return Blackbox?.Exert(BlackboxRegistry.GetBlackbox(other), message, methodName) ?? message;
        }

        [Conditional("BLACKBOX")]
        public void Exerted<T>(T other, [InterpolatedStringHandlerArgument("")] ref WriteHandler handler, [CallerMemberName] string methodName = "") where T : class
        {
            if (!handler.ShouldLog) return;
            ExertedMessage(other, handler.GetTextAndClear(), methodName);
        }
        [Conditional("BLACKBOX")]
        public void Exerted<T>(T other, string message, [CallerMemberName] string methodName = "") where T : class
        {
            ExertedMessage(other, message, methodName);
        }
        public string ExertedMessage<T>(T other, string message, [CallerMemberName] string methodName = "") where T : class
        {
            return BlackboxHandle.Of(other).ExertMessage(Owner, message, methodName);
        }

        public DisposableHandle ExertScope<T>(T other, [InterpolatedStringHandlerArgument("")] ref WriteHandler handler, [CallerMemberName] string methodName = "") where T : class
        {
            if (!handler.ShouldLog) return default;
            return ExertScope(other, handler.GetTextAndClear(), methodName);
        }
        public DisposableHandle ExertScope<T>(T other, string message, [CallerMemberName] string methodName = "") where T : class
        {
            return Blackbox?.ExertScope(BlackboxRegistry.GetBlackbox(other), message, methodName) ?? default;
        }

        public DisposableHandle ExertedScope<T>(T other, [InterpolatedStringHandlerArgument("")] ref WriteHandler handler, [CallerMemberName] string methodName = "") where T : class
        {
            if (!handler.ShouldLog) return default;
            return ExertedScope(other, handler.GetTextAndClear(), methodName);
        }
        public DisposableHandle ExertedScope<T>(T other, string message, [CallerMemberName] string methodName = "") where T : class
        {
            return Blackbox?.ExertedScope(BlackboxRegistry.GetBlackbox(other), message, methodName) ?? default;
        }

        [Conditional("BLACKBOX")]
        public void WriteOrExerted<T>([InterpolatedStringHandlerArgument("")] ref WriteHandler handler, T other, [CallerMemberName] string methodName = "") where T : class
        {
            if (!handler.ShouldLog) return;
            WriteOrExertedMessage(handler.GetTextAndClear(), other, methodName);
        }
        public string WriteOrExertedMessage<T>(string message, T other, [CallerMemberName] string methodName = "") where T : class
        {
            if (other == null) return WriteMessage(message, methodName); 
            return ExertedMessage(other, message, methodName);
        }

        public DisposableHandle WriteOrExertedScope<T>([InterpolatedStringHandlerArgument("")] ref WriteHandler handler, T other, [CallerMemberName] string methodName = "") where T : class
        {
            if (!handler.ShouldLog) return default;

            if (other == null) return WriteScope(ref handler, methodName);
            return ExertedScope(other, ref handler, methodName);
        }
        public DisposableHandle WriteOrExertedScope<T>(string message, T other, [CallerMemberName] string methodName = "") where T : class
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
            WriteMessage($"[STACK TRACE]\n{new StackTrace(true)}\n");

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
