using System;
using System.Linq;
using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class Hit : BTNode<Monster, MonsterBlackboard>
    {
        // Internal
        private readonly string monsterAction;


        // Content
        public Hit(MonsterAction monsterAction = MonsterAction.Hit) : this(monsterAction.ToString()) { }
        public Hit(string monsterAction)
        {
            IsSelectable = false;
            AbortPolicy = AbortPolicies.LowerPriority;
            HierarchyMode = HierarchyMode.Selector;

            this.monsterAction = monsterAction;
        }

        public override bool CheckCondition() => !Owner.TryGetCurrentAction(out var _monsterAction) || _monsterAction != monsterAction;

        protected override void OnOpen(object[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs), CtxHit("Inputs 인자는 null일 수 없습니다."));

            if (inputs.Length != 1 ||  inputs[0] is not int damage)
                throw new ArgumentException(CtxHit($"Inputs 인자는 damage(int)를 담고 있는 크기 1의 배열이여야 합니다.\n" +
                    $"입력값: {string.Join(", ", inputs.Select(i => i?.ToString() ?? null))}"), nameof(inputs));

            if (damage < 0)
                throw new ArgumentOutOfRangeException(nameof(damage),
                    $"Damage는 0 이상이어야 하지만 '{damage}'이(가) 입력되었습니다.");


            if (!Owner.TryDoAction(monsterAction, out var reason, result => Complete(result), playTime: Owner.InvincibleDuration))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{monsterAction} 행동에 실패하였기 때문에 Hit 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
                return;
            }

            Owner.HP -= damage;
        }

        private string CtxHit(string message) => Owner.Ctx($"BTNode.Hit: {message}");
    }
}
