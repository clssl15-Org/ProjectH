using System;

namespace Actors.Monsters
{
    public class MonsterConditionData : IMonsterConditionData
    {
        public MonsterCondition Condition { get; init; }
        public object Payload { get; init; } = null;
        public event Action Callback;


        public MonsterConditionData()
        {
        }

        public MonsterConditionData(
            MonsterCondition condition,
            object payload = null)
        {
            Condition = condition;
            Payload = payload;
        }

        internal void Complete()
        {
            Callback?.Invoke();
            Callback = null;
        }
    }
}
