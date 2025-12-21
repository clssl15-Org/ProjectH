using System;
using System.Linq;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class Ghost
    {
        private class GhostAttackBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            public GhostAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(params object[] _)
            {
                var ower = (Ghost)Owner;
                AttackMode mode;

                if (ower._attackMode == AttackMode.Any)
                    mode = UnityEngine.Random.Range(0f, 1f) switch
                    {
                        var i when 0f <= i && i < 0.75f => AttackMode.RangedAttack,
                        var i when i <= 1f => AttackMode.ExplosiveAttack,
                        var i => throw new ArgumentOutOfRangeException(
                            nameof(i), i, Owner.FormatLogMessage($"공격 패턴의 범위는 0 이상 1 이하여야 합니다."))
                    };
                else
                    mode = ower._attackMode;

                // 폭발 공격 중에는 플레이어 상호작용 없음
                if (mode == AttackMode.ExplosiveAttack)
                    Owner.IgnorePlayerInteraction = true;


                string action;
                object[] inputs = null;

                switch (mode)
                {
                    case AttackMode.RangedAttack:
                        action = "Attack_1";
                        inputs = new object[]
                        {
                            null,
                            (Func<IWeapon, Func<bool>>)(weapon =>
                            {
                                if (!weapon.gameObject.TryGetComponent<TriggerContactHandler>(out var contactHandler))
                                    throw new ArgumentException(
                                        Owner.FormatLogMessage($"{nameof(weapon)}은(는) '{nameof(TriggerContactHandler)}' 컴포넌트를 가지고 있어야 합니다."),
                                        nameof(weapon));

                                return () => !contactHandler.Collisions.Any();
                            })};
                        break;

                    case AttackMode.ExplosiveAttack:
                        action = "Attack_2";
                        break;

                    default:
                        throw new ArgumentOutOfRangeException(
                            nameof(mode), mode, Owner.FormatLogMessage($"알 수 없는 공격 패턴이 입력되었습니다."));
                }

                if (!Owner.TryDoAction(new(action, result => Complete(result), inputs), out var reason))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{action} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }

                Blackboard.Committing = true;
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                Owner.IgnorePlayerInteraction = false;
                Blackboard.Committing = false;
            }
        }
    }
}
