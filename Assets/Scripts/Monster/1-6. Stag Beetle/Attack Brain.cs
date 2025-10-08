using System;
using UniEngine.StateMachines.BT;
using UnityEngine;

public partial class StagBeetle : Monster
{
    private class StagBeetleAttack : BTNode<Monster, MonsterBlackboard>
    {
        public StagBeetleAttack() : base(name: MonsterActionType.Attack.ToString()) { }

        protected override void OnOpen(params object[] _)
        {
            var ower = (StagBeetle)Owner;
            AttackMode mode;

            if (ower._attackMode == AttackMode.Any)
                mode = UnityEngine.Random.Range(0, Owner.HP < Owner.MaxHP ? 3 : 2) switch
                {
                    0 => AttackMode.RollAttack,
                    1 => AttackMode.SpikeAttack,
                    2 => AttackMode.Roar,
                    var i => throw new InvalidOperationException(
                        Owner.Ctx($"공격 패턴의 범위는 [0..2]이여야 하지만 '{i}'이(가) 입력되었습니다."))
                };
            else
                mode = ower._attackMode;


            if (!Owner.TryDoAction(mode.ToString(), out var reason, result => Complete(result)))
            {
                Debug.LogWarning(Owner.Ctx(
                    $"{mode.ToString()} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

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
