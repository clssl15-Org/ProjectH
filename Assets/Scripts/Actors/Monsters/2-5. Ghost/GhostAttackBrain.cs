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
            // Internal
            private MonsterConditionData _notification;


            // Content
            public GhostAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(object[] _)
            {
                var owner = (Ghost)Owner;
                AttackMode mode = owner._attackMode.Resolve(AttackMode.Any);

                if (mode == AttackMode.Any)
                    mode = UnityEngine.Random.Range(0f, 1f) switch
                    {
                        var i when 0f <= i && i < 0.75f => AttackMode.RangedAttack,
                        var i when i <= 1f => AttackMode.ExplosiveAttack,
                        var i => throw new ArgumentOutOfRangeException(
                            nameof(i), i, Owner.FormatLogMessage($"공격 패턴의 범위는 0 이상 1 이하여야 합니다."))
                    };

                // 폭발 공격 중에는 플레이어 상호작용 없음
                if (mode == AttackMode.ExplosiveAttack)
                    Owner.IgnorePlayerInteraction = true;


                string action;
                object[] inputs = null;
                bool isRanged;

                switch (mode)
                {
                    case AttackMode.RangedAttack:
                        action = "Attack_1";
                        isRanged = true;

                        _notification = new MonsterConditionData(
                            MonsterCondition.Attack,
                            new MonsterAttackData(action, isRanged));

                        inputs = new object[]
                        {
                            null,
                            new AttackWithWeapon.Payload
                            {
                                GetCheckCondition = weapon =>
                                {
                                    if (!weapon.gameObject.TryGetComponent<TriggerContactHandler>(out var contactHandler))
                                        throw new ArgumentException(
                                            Owner.FormatLogMessage($"{nameof(weapon)}은(는) '{nameof(TriggerContactHandler)}' 컴포넌트를 가지고 있어야 합니다."),
                                            nameof(weapon));

                                    return () => !contactHandler.Collisions.Any();
                                },
                                MonsterConditionData = _notification
                            }
                        };

                        break;

                    case AttackMode.ExplosiveAttack:
                        action = "Attack_2";
                        isRanged = false;

                        _notification = new MonsterConditionData(
                            MonsterCondition.Attack,
                            new MonsterAttackData(action, isRanged));

                        inputs = new object[]
                        {
                            new AttackWithWeapon.Payload(MonsterConditionData: _notification)
                        };
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
                    return;
                }


                Blackboard.IsCommitting = true;
                Owner.NotifyCondition(_notification);
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                Owner.IgnorePlayerInteraction = false;
                Blackboard.IsCommitting = false;

                _notification?.Complete();
                _notification = null;
            }
        }
    }
}
