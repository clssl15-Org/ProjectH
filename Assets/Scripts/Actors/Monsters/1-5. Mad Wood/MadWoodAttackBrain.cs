using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class MadWood
    {
        private class MadWoodAttackBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            public MadWoodAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(object[] _)
            {
                var owner = (MadWood)Owner;

                var currentAttackMode = owner._previousAttackMode == AttackMode.DefaultAttack
                    ? AttackMode.LandAttack
                    : AttackMode.DefaultAttack;

                if (!owner.TryDoAction(new MonsterActionPlayInfo(
                    Name: currentAttackMode.ToString(),
                    Callback: result => Complete(result)),
                    out var reason))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"({nameof(MadWoodAttackBrain)}) {currentAttackMode.ToString()} 행동에 실패하였기 때문에 Attack 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }

                owner._previousAttackMode = currentAttackMode;
                Blackboard.IsCommitting = true;
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                Blackboard.IsCommitting = false;
            }
        }
    }
}
