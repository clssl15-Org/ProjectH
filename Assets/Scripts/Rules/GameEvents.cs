using System;
using BlackThunder.BlackboxSystem;

namespace Rules
{
    public static class GameEvents
    {
        public static Action OnMonsterDied;

        public static void NotifyMonsterDied()
        {
            using var _ = BlackboxHandle.Of(typeof(GameEvents)).Scope("몬스터 사망 이벤트를 알립니다.");

            OnMonsterDied?.Invoke();
        }
    }
}
