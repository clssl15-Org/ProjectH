using System;
using System.Linq;

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

    public class MonsterConditionData
    {
        public MonsterCondition Condition { get; init; }
        /// <summary>
        /// 인자 종류
        /// <list type="bullet">
        ///   <item><description>Damaged: <see cref="DamageInfo"/></description></item>
        ///   <item><description>Attack: <see cref="MonsterAttackData"/></description></item>
        ///   <item><description>Dying: children (<see cref="IMonster"/>[])</description></item>
        /// </list>
        /// </summary>
        public object Payload { get; init; } = null;
        public event Action Callback;

        public MonsterConditionData() { }
        public MonsterConditionData(MonsterCondition condition, object payload = null)
        {
            Condition = condition;
            Payload = payload;
        }

        public bool Is(params MonsterCondition[] conditions) =>
            conditions.Contains(Condition);

        /// <summary>
        /// 이 메서드는 발행자만 호출할 수 있습니다.
        /// </summary>
        public void Complete()
        {
            Callback?.Invoke();
            Callback = null;
        }
    }


    public enum AttackEvent
    {
        None,
        Started,
        HitPlayer,
        Finished,
    }

    public class MonsterAttackData
    {
        public string Name { get; }
        public bool IsRangedAttack { get; }
        public event Action<AttackEvent> EventOccurred;

        public MonsterAttackData(string name, bool isRangedAttack)
        {
            Name = name;
            IsRangedAttack = isRangedAttack;
        }

        public void NotifyEvent(AttackEvent phase) => EventOccurred?.Invoke(phase);
    }
}
