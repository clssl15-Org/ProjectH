using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class Attack : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Internal
        private readonly string _monsterAction;
        private readonly bool _isRangedAttack;
        private MonsterConditionData _notification;


        // Content
        public Attack(bool isRangedAttack) : this(MonsterActionType.Attack, isRangedAttack) { }
        public Attack(MonsterActionType monsterAction, bool isRangedAttack) : this(monsterAction.ToString(), isRangedAttack) { }
        public Attack(string monsterAction, bool isRangedAttack)
        {
            _monsterAction = monsterAction;
            _isRangedAttack = isRangedAttack;
        }

        protected override void OnOpen(object[] _)
        {
            _notification = new MonsterConditionData(
                MonsterCondition.Attack,
                new MonsterAttackData(Name, _isRangedAttack));

            if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result => Complete(result),
                Inputs: new[]
                {
                    null, // AnimationComponent Input
                    new AttackWithWeapon.Payload(MonsterConditionData: _notification) // AttackWithWeapon Input
                }), 
                out var reason,
                allowRestart: true))
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {nameof(Attack)} 상태로 진입할 수 없습니다.\n{reason}"));

                Complete(false);
                return;
            }
            
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
