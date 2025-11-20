using System;
using System.Linq;
using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class Hit : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;
        private bool _doKnockback = true;

        private MonsterConditionData _notification;


        // Content
        public Hit(MonsterActionType monsterAction = MonsterActionType.Hit, bool doKnockback = true) : this(monsterAction.ToString(), doKnockback) { }
        public Hit(string monsterAction, bool doKnockback = true)
        {
            IsSelectable = false;
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

            Owner.HP -= damageInfo.Damage;

            _notification = new MonsterConditionData(MonsterCondition.Damage);
            Owner.NotifyCondition(_notification);

            if (_doKnockback && damageInfo.HasKnockback)
                Owner.Knockback(damageInfo.Direction, damageInfo.KnockbackForce);

            if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result => Complete(result),
                Inputs: new object[] { Owner.StatsInfo.InvincibleDuration }),
                out var reason))
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
                return;
            }
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            _notification?.Complete();
            _notification = null;
        }

        private string CtxHit(string message) => Owner.FormatLogMessage($"BTNode.Hit: {message}");
    }
}
