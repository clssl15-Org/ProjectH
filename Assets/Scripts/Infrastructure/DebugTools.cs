using UnityEngine;

namespace Infrastructure
{
    public static class DebugTools
    {
        public static bool IsDebugMode
        {
            get
            {
#if DEBUG_MODE
                return true;
#else
                return false;
#endif
            }
        }

        public static KeyCode Resolve(this KeyCode keyCode) =>
            Resolve(keyCode, KeyCode.None);

        public static T Resolve<T>(this T target, T onRelease)
        {
#if DEBUG_MODE
            return target;
#else
            return onRelease;
#endif
        }
    }
}
