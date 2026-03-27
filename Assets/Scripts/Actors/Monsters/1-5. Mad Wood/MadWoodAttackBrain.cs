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
            private MonsterConditionData _notification;

            public MadWoodAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(object[] _)
            {
                var owner = (MadWood)Owner;
                var currentAttackMode = owner._previousAttackMode == AttackMode.DefaultAttack
                    ? AttackMode.LandAttack
                    : AttackMode.DefaultAttack;

                _notification = new MonsterConditionData(
                    MonsterCondition.Attack,
                    new MonsterAttackData(currentAttackMode.ToString(), false));

                if (!owner.TryDoAction(new MonsterActionPlayInfo(
                    Name: currentAttackMode.ToString(),
                    Callback: result => Complete(result),
                    Inputs: new[]
                    {
                        null, // AnimationComponent Input
                        new AttackWithWeapon.Payload(MonsterConditionData: _notification) // AttackWithWeapon Input
                    }),
                    out var reason))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"({nameof(MadWoodAttackBrain)}) '{currentAttackMode.ToString()}' 행동에 실패하였기 때문에 Attack 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }

                owner._previousAttackMode = currentAttackMode;
                Blackboard.IsCommitting = true;

                Owner.NotifyCondition(_notification);
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                Blackboard.IsCommitting = false;

                _notification?.Complete();
                _notification = null;
            }
        }
    }
}
