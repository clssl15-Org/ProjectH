using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Verbelion
    {
        private class VerbelionDeadBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            // Internal
            private Verbelion Verbelion => (Verbelion)Owner;

            private readonly string _isOnAir;
            private readonly string _deadAir;
            private readonly string _deadGround;
            private MonsterConditionData _notification;


            // Content
            public VerbelionDeadBrain(
                string isOnAir = "IsOnAir",
                string deadAir = "DeadAir",
                string deadGround = "DeadGround")
                : base(name: "Dead")
            {
                _isOnAir = isOnAir;
                _deadAir = deadAir;
                _deadGround = deadGround;
            }

            public override bool CheckCondition() => Owner.IsAlive;

            protected override void OnOpen(object[] _)
            {
                Owner.IsAlive = false;
                Owner.IgnorePlayerInteraction = true;

                _notification = new MonsterConditionData(MonsterCondition.Dying);
                Owner.NotifyCondition(_notification);

                string actionToDo;
                object[] inputs;

                if ((bool)Blackboard.Properties[_isOnAir])
                {
                    actionToDo = _deadAir;

                    var targetPosition = new Vector2(
                        Verbelion.transform.position.x,
                        Verbelion._groundPoints[0].transform.position.y);

                    inputs = new object[]
                    { 
                        null,
                        targetPosition
                    };
                }
                else
                {
                    actionToDo =  _deadGround;
                    inputs = null;
                }

                if (!Owner.TryDoAction(new(
                    Name: actionToDo,
                    Callback: result => Complete(),
                    Inputs: inputs),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{actionToDo} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                }
            }

            protected override void OnHalt(DetailedNodeStatus _)
            {
                _notification?.Complete();
                _notification = null;
            }
        }
    }
}
