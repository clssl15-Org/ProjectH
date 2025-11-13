using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    public partial class EyeballMonsterSecondPhase
    {
        private class SecondPhaseBornBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            // Internal
            private bool _isBorn = false;
            private float? _remainingToMove = null;


            // Content
            public SecondPhaseBornBrain() : base(name: "Born") { }

            public override bool CheckCondition() => !_isBorn;

            protected override void OnOpen(params object[] inputs)
            {
                _isBorn = true;

                if (!Owner.TryDoAction(new(
                    Name: Name,
                    Callback: result =>
                    {
                        if (!result)
                        {
                            Complete(result);
                            return;
                        }

                        var owner = (EyeballMonsterSecondPhase)Owner;
                        _remainingToMove = Random.Range(owner._minDelayAfterBorn, owner._maxDelayAfterBorn);
                    }),
                    out var reason))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{Name} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

                    Complete(false);
                    return;
                }
            }

            protected override void OnTick()
            {
                if (!_remainingToMove.HasValue)
                    return;

                _remainingToMove -= Time.deltaTime;
                if (_remainingToMove <= 0) Complete();
            }
        }
    }
}
