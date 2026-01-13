using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BlackboxSystem
{
    public readonly struct BlackboxHandle
    {
        // Forwarders
        public object Owner
        {
            get
            {
                ThrowIfInvalid();
                return _blackbox.Owner;
            }
        }
        public string OwnerString
        {
            get
            {
                ThrowIfInvalid();
                return _blackbox.OwnerString;
            }
        }
        public long Id
        {
            get
            {
                ThrowIfInvalid();
                return _blackbox.Id;
            }
        }

        public static string LogDirectory
        {
            get => Infrastructure.LogDirectory;
            set => Infrastructure.LogDirectory = value;
        }
        public static Action<string> Logger
        {
            get => Infrastructure.Logger;
            set => Infrastructure.Logger = value;
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


        private readonly Blackbox _blackbox;
        internal BlackboxHandle(Blackbox blackbox) => _blackbox = blackbox;

        #region Static Methods
        public static void Initialize(Action<string> logger, bool strongReference = false) => Initialize(string.Empty, logger, strongReference);
        public static void Initialize(string logDirectory, Action<string> logger, bool strongReference = false)
        {
            LogDirectory = logDirectory;
            Logger = logger;
            StrongReference = strongReference;
        }

        /// <summary>
        /// Force to initialize all black box records. This feature should be used carefully.
        /// </summary>
        public static void ForceReset() => BlackboxRegistry.ForceReset();
        #endregion

        public static BlackboxHandle Of(object subject)
        {
#if !BLACKBOX
            return default;
#else
            if (subject == null)
                throw new ArgumentNullException(
                    nameof(subject), "[BlackboxHandle] Subject cannot be null");

            return new BlackboxHandle(BlackboxRegistry.GetBlackbox(subject));
#endif
        }

        public string Write(object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = message?.ToString() ?? "null";
            if (_blackbox == null) return messageStr;

            return _blackbox.Write(messageStr, methodName);
        }
        public DisposableHandle WriteScope(object message, [CallerMemberName] string methodName = "")
        {
            if (_blackbox == null) return default;
            var messageStr = message?.ToString() ?? "null";

            return _blackbox.WriteScope(messageStr, methodName);
        }

        public string Exert(object other, object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = message?.ToString() ?? "null";
            if (_blackbox == null) return messageStr;

            var otherBlackbox = BlackboxRegistry.GetBlackbox(other);
            return _blackbox.Exert(otherBlackbox, messageStr, methodName);
        }
        public DisposableHandle ExertScope(object other, object message, [CallerMemberName] string methodName = "")
        {
            if (_blackbox == null) return default;
            var messageStr = message?.ToString() ?? "null";

            var otherBlackbox = BlackboxRegistry.GetBlackbox(other);
            return _blackbox.ExertScope(otherBlackbox, messageStr, methodName);
        }

        public string Exerted(object other, object message, [CallerMemberName] string methodName = "")
        {
            var messageStr = message?.ToString() ?? "null";
            if (_blackbox == null) return messageStr;

            return BlackboxHandle.Of(other).Exert(Owner, message, methodName);
        }
        public DisposableHandle ExertedScope(object other, object message, [CallerMemberName] string methodName = "")
        {
            if (_blackbox == null) return default;
            var messageStr = message?.ToString() ?? "null";

            return BlackboxHandle.Of(other).ExertScope(Owner, message, methodName);
        }

        public string CrashExport(string message, int recursionDepth = 5, bool openLog = true)
        {
            Write($"[CRASH] {message}");
            Write($"[STACK TRACE]\n{new StackTrace(true).ToString()}\n");

            Export(recursionDepth, true, openLog);
            return message;
        }
        public void Export(int recursionDepth = 5, bool openLog = true) => Export(recursionDepth, false, openLog);
        private void Export(int recursionDepth, bool isCrash, bool openLog)
        {
            if (string.IsNullOrWhiteSpace(Infrastructure.LogDirectory))
                throw new InvalidOperationException(
                    $"[BlackboxHandle] {nameof(Infrastructure.LogDirectory)} is empty. " +
                    $"Use 'Export(string path, int recursionDepth, bool openLog)' method instead.");

            if (_blackbox == null)
                return;

            if (!_blackbox.TryPrint(recursionDepth, out var result))
            {
                var message = $"[BlackboxHandle] Cannot print because the print is already been done.";

                Infrastructure.Log(message);
                return;
            }


            Directory.CreateDirectory(Infrastructure.LogDirectory);

            var fileName = $"Blackbox {TrimSmart(_blackbox.OwnerString)} ({_blackbox.Id}).txt";
            if (isCrash) fileName = "[CRASH] " + fileName;

            var fullPath = Path.Combine(Infrastructure.LogDirectory, fileName);
            File.WriteAllText(fullPath, result);

            Infrastructure.Log($"[BlackboxHandle] Log successfully exported to '{fullPath}'");

            if (openLog)
            {
                try
                {
                    Process.Start(new ProcessStartInfo(fullPath) { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    Infrastructure.Log(
                        $"[BlackboxHandle] Failed to open the file automatically.\n{ex.ToString()}");
                }
            }


            string TrimSmart(string input)
            {
                if (string.IsNullOrEmpty(input))
                    return input;

                var invalidChars = Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars()).Distinct();
                foreach (var c in invalidChars)
                    input = input.Replace(c, '.');

                if (input.Length <= 10)
                    return input;

                var start = input[..5];
                var end = input.Substring(input.Length - 5, 5);

                return $"{start}...{end}";
            }
        }

        private void ThrowIfInvalid()
        {
            if (_blackbox == null)
                throw new InvalidOperationException(
                    "[BlackboxHandle] Invalid handle. Use BlackboxHandle.Of(subject) to create it.");
        }
    }
}
