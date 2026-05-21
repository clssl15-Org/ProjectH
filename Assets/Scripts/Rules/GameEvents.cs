using System;

namespace Rules
{
    public static class GameEvents
    {
        public static Action OnMonsterDied;

        public static void NotifyMonsterDied() => OnMonsterDied?.Invoke();
    }
}