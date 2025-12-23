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
        Die,
    }

    public interface IMonsterConditionData
    {
        MonsterCondition Condition { get; }
        /// <summary>
        /// 인자 종류
        /// <list type="bullet">
        ///   <item><description>Damaged: <see cref="DamageInfo"/></description></item>
        ///   <item><description>Attack: IsRangedAttack (<see cref="bool"/>)</description></item>
        /// </list>
        /// </summary>
        object Payload { get; }
        event Action Callback;

        bool Is(params MonsterCondition[] conditions);
    }
}
