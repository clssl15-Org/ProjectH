using System;
using Infrastructure.StateMachines.BT;

namespace Actors.Monsters.Brains
{
    internal class Alive : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        private Action _opened;

        public Alive(Action opened = null)
        {
            AbortPolicies = AbortPolicies.Self | AbortPolicies.LowerPriority;
            HierarchyMode = HierarchyMode.Selector;
            LoopType = LoopType.Forced;

            _opened = opened;
        }

        public override bool CheckCondition()
        {
            if (!Owner.IsAlive)
                return false;
            if (Owner.HP <= 0)
                return false;

            return true;
        }

        protected override void OnOpen(params object[] _)
        {
            _opened?.Invoke();
        }
    }
}
