using System;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BlackboxSystem
{
    internal static class BlackboxRegistry
    {
        private static ConditionalWeakTable<object, Blackbox> _subjects = new();
        private static object _lock = new();

        public static Blackbox GetBlackbox(object subject)
        {
            if (subject == null)
                throw new ArgumentNullException(nameof(subject), "[BlackboxRegistry] Subject cannot be null");

            return _subjects.GetValue(subject, key => new Blackbox(key, Infrastructure.StrongReference));
        }

        internal static bool Contains(object subject)
        {
            if (subject == null)
                throw new ArgumentNullException(nameof(subject), "[BlackboxRegistry] Subject cannot be null");

            return _subjects.TryGetValue(subject, out _);
        }

        internal static int Count()
        {
            lock (_lock)
            {
                return _subjects.Count();
            }
        }

        public static void ForceReset()
        {
            lock (_lock)
            {
                _subjects = new();
                Blackbox.ForceResetStaticProperties();
            }
        }
    }
}
