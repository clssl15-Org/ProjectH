using System;
using UniEngine.StateMachines.BT;
using UnityEngine;

public partial class Ghost
{
    private class GhostAttack : BTNode<Monster, MonsterBlackboard>
    {
        public GhostAttack() : base(name: MonsterActionType.Attack.ToString()) { }

        protected override void OnOpen(params object[] _)
        {
            var ower = (Ghost)Owner;
            AttackMode mode;

            if (ower._attackMode == AttackMode.Any)
                mode = UnityEngine.Random.Range(0f, 1f) switch
                {
                    var i when 0f <= i && i < 0.75f => AttackMode.RangedAttack,
                    var i when i <= 1f => AttackMode.ExplosiveAttack,
                    var i => throw new InvalidOperationException(
                        Owner.Ctx($"공격 패턴의 범위는 0 이상 1 이하여야 하지만 '{i}'이(가) 입력되었습니다."))
                };
            else
                mode = ower._attackMode;


            var action = mode switch
            {
                AttackMode.RangedAttack => "Attack_1",
                AttackMode.ExplosiveAttack => "Attack_2",
                _ => throw new InvalidOperationException(
                    Ctx($"알 수 없는 공격 패턴 '{mode}'이(가) 입력되었습니다."))
            };


            if (!Owner.TryDoAction(action, out var reason, result => Complete(result)))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{action} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
            }

            Blackboard.Committing = true;
        }

        protected override void OnHalt(DetailedNodeStatus _)
        {
            Blackboard.Committing = false;
        }
    }
}
