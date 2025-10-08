using System;
using UniEngine.StateMachines.FSM;

namespace MonsterActions
{
    internal abstract class MonsterAction : Work
    {
        // Front
        public Monster Owner
        {
            get
            {
                var current = Parent;

                do
                {
                    if (current is MonsterActionController currentParent)
                        return currentParent.Owner;

                    current = current.Parent;
                } while (current != null);

                throw new InvalidOperationException(
                    Ctx($"{nameof(MonsterActionController)} 형식의 Parent를 찾지 못하였습니다."));
            }
        }


        // Internal
        private Action<ActionResult> _callback;
        private float? _playTime;

        protected float? RemainingTime { get; private set; }


        // Content
        public MonsterAction(MonsterActionType monsterAction) : this(monsterAction.ToString()) { }
        public MonsterAction(string name) : base(name) { }

        protected override void OnEnter(params object[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs),
                    Ctx($"{nameof(inputs)}은(는) null일 수 없습니다."));

            if (inputs.Length < 3)
                throw new ArgumentException(
                    Ctx($"{nameof(inputs)}은(는) 3 이상의 길이를 가져야 하지만 길이 '{inputs.Length}'을 가진 인자가 입력되었습니다."), nameof(inputs));


            _callback = (Action<ActionResult>)inputs[0];
            _playTime = (float?)inputs[1];

            RemainingTime = _playTime;
        }
    }
}
