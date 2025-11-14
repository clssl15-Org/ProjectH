using Actors.Monsters.Actions;
using Infrastructure.StateMachines.BT;
using UnityEngine;
    
namespace Actors.Monsters.Brains
{
    internal class Dead : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Front
        public float StayTimeAfterFinised { get; set; } = 0f;

        // Internal
        private readonly string _monsterAction;
        private MonsterConditionData _notification;


        // Content
        public Dead(MonsterActionType monsterAction = MonsterActionType.Dead) : this(monsterAction.ToString()) { }
        public Dead(string monsterAction) => _monsterAction = monsterAction;

        public override bool CheckCondition() => Owner.IsAlive;

        protected override void OnOpen(object[] _)
        {
            Owner.IsAlive = false;
            Owner.Collider.excludeLayers = LayerMask.GetMask("Player");

            _notification = new MonsterConditionData(MonsterCondition.Die);
            Owner.NotifyCondition(_notification);

            if (!Owner.TryDoAction(new(
                Name: _monsterAction,
                Callback: result => Complete(),
                Inputs: new[]
                {
                    new PlayAnimation.AnimationPlayInfo(DelayAfterPlay: StayTimeAfterFinised)
                }),
                out var reason,
                allowRestart: true))
            {
                Debug.LogWarning(Owner.FormatLogMessage(
                    $"{_monsterAction} 행동에 실패하였기 때문에 {GetType().Name} 상태로 진입할 수 없습니다.\n{reason}"));

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
