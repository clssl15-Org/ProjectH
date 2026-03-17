using System;
using System.Linq;

namespace Actors.Monsters
{
    public class MonsterConditionData : IMonsterConditionData
    {
        public MonsterCondition Condition { get; init; }
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

        public void Complete()
        {
            Callback?.Invoke();
            Callback = null;
        }
    }
}
