using System;

namespace Actors
{
    public enum MonsterCondition
    {
        None,
        General,
        Heal,
        Attack,
        Damage,
        Die,
    }

    public interface IMonsterConditionData
    {
        MonsterCondition Condition { get; }
        object Payload { get; }
        event Action Callback;

        bool Is(params MonsterCondition[] conditions);
    }
}
