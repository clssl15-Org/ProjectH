using System;

namespace Actors
{
    public enum MonsterCondition
    {
        None,
        General,
        PlayerDetected,
        Heal,
        Attack,
        Damaged,
        Dying,
        Died,
    }

    public interface IMonsterConditionData
    {
        MonsterCondition Condition { get; }
        /// <summary>
        /// 인자 종류
        /// <list type="bullet">
        ///   <item><description>Damaged: <see cref="DamageInfo"/></description></item>
        ///   <item><description>Attack: <see cref="Monsters.MonsterAttackData"/></description></item>
        /// </list>
        /// </summary>
        object Payload { get; }
        event Action Callback;

        bool Is(params MonsterCondition[] conditions);
        /// <summary>
        /// 이 메서드는 발행자만 호출할 수 있습니다.
        /// </summary>
        void Complete();
    }
}
