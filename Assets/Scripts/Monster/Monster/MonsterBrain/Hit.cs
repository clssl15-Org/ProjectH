using System;
using System.Linq;
using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class Hit : BTNode<Monster, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;
        private bool _doKnockback = true;


        // Content
        public Hit(MonsterActionType monsterAction = MonsterActionType.Hit, bool doKnockback = true) : this(monsterAction.ToString(), doKnockback) { }
        public Hit(string monsterAction, bool doKnockback = true)
        {
            IsSelectable = false;
            AbortPolicy = AbortPolicies.LowerPriority;
            HierarchyMode = HierarchyMode.Selector;

            _monsterAction = monsterAction;
            _doKnockback = doKnockback;
        }

        public override bool CheckCondition() => !Owner.TryGetCurrentAction(out var monsterAction) || _monsterAction != monsterAction;

        protected override void OnOpen(object[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs), CtxHit("inputs 인자는 null일 수 없습니다."));

            if (inputs.Length != 1 ||  inputs[0] is not DamageInfo damageInfo)
                throw new ArgumentException(CtxHit($"inputs 인자는 damage({typeof(DamageInfo).Name})을(를) 담고 있는 크기 1의 배열이여야 합니다.\n" +
                    $"입력값: {string.Join(", ", inputs.Select(i => i?.ToString() ?? null))}"), nameof(inputs));


            if (!Owner.TryDoAction(_monsterAction, out var reason, result => Complete(result), playTime: Owner.InvincibleDuration))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
                return;
            }


            Owner.HP -= damageInfo.Damage;

            if (_doKnockback && damageInfo.HasKnockback)
                Owner.Knockback(damageInfo.Direction, damageInfo.KnockbackForce);
        }

        private string CtxHit(string message) => Owner.Ctx($"BTNode.Hit: {message}");
    }
}
