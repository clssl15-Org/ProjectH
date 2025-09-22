using System;
using UniEngine.StateMachines.FSM;
using UnityEngine;

namespace MonsterActions
{
    internal class MonsterActionState : Work
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
                    Ctx($"{nameof(Owner)}을(를) 가지고 있는 Parent를 찾지 못하였습니다."));
            }
        }
        public string AnimationName { get; protected set; } = MonsterAction.None.ToString();

        public float? Playtime { get; private set; } = null;
        protected float? MainAnimationRemainingTime { get; set; }

        protected Action<ActionResult> Callback { get; set; }
        protected float StayTimeAfterFinished { get; private set; }
        protected bool isCompleted;

        protected Action PlayActionEnter, PlayActionExit;
        protected Action AfterActionEnter, AfterActionExit;

        // Internal
        private string trigger;
        private float? start;
        private float? end;


        // Content
        public MonsterActionState(MonsterAction monsterAction, string trigger = null, float? start = null, float? end = null) : this(monsterAction.ToString(), trigger, start, end) { }
        public MonsterActionState(string monsterAction, string trigger = null, float? start = null, float? end = null) : base(monsterAction)
        {
            AnimationName = monsterAction;
            this.trigger = trigger;
            this.start = start;
            this.end = end;

            Initialize();
        }
        protected virtual void Initialize()
        {
            AddChild(new PlayAction(), true);
            AddChild(new AfterAction());
        }

        protected override void OnEnter(params object[] inputs)
        {
            SetInputs(inputs);

            if (!HasAnimation())
            {
                Owner.ActionController.StopAnimator();
                MainAnimationRemainingTime = (Playtime.HasValue && Playtime.Value >= 0) ? Playtime : null;
                return;
            }

            if (!Owner.Animator.TryFindClip(AnimationName, out var clip))
            {
                throw new ArgumentException(Owner.Ctx($"애니메이터에 '{AnimationName}' 클립이 없습니다."));
            }

            isCompleted = false;


            float? duration = null;

            if (Playtime.HasValue && Playtime.Value >= 0)
                duration = Playtime.Value;
            else if (!clip.isLooping)
                duration = clip.length;

            if (duration.HasValue)
            {
                duration -= start.GetValueOrDefault(0f);
                duration -= end.GetValueOrDefault(0f);
            }

            MainAnimationRemainingTime = duration;


            if (string.IsNullOrEmpty(trigger))
                Owner.Animator.Play(clip.name, -1, start.GetValueOrDefault(0f) / clip.length);
            else
            {
                if (start.HasValue || end.HasValue)
                    throw new InvalidOperationException(
                        Owner.Ctx($"현재 Trigger 방식 재생에서는 {nameof(start)}/{nameof(end)} 속성을 사용할 수 없습니다."));

                Owner.Animator.SetTrigger(trigger);
            }
        }
        protected void SetInputs(params object[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs), Ctx(
                    $"{nameof(inputs)}은(는) null일 수 없습니다."));

            if (inputs.Length < 3)
                throw new ArgumentException(Ctx(
                    $"{nameof(inputs)}은(는) 3 이상의 길이를 가져야 하지만 길이 '{inputs.Length}'을 가진 인자가 입력되었습니다."), nameof(inputs));

            Callback = (Action<ActionResult>)inputs[0];
            Playtime = (float?)inputs[1];
            StayTimeAfterFinished = (float)inputs[2];
        }

        protected void Complete()
        {
            isCompleted = true;
            Exit();
        }

        protected override void OnExit()
        {
            var callback = Callback;
            Callback = null;

            callback?.Invoke(new(isCompleted
                ? ActionResult.ResultType.Success
                : ActionResult.ResultType.Interrupted));
        }

        private bool HasAnimation()
            => !string.IsNullOrWhiteSpace(AnimationName)
            && !string.Equals(AnimationName, MonsterAction.None.ToString());


        // Substates
        internal class PlayAction : Work<MonsterActionState>
        {
            protected override void OnEnter(params object[] _)
            {
                Parent.PlayActionEnter?.Invoke();
            }

            protected override void OnUpdate()
            {
                if (!Parent.MainAnimationRemainingTime.HasValue)
                    return;


                Parent.MainAnimationRemainingTime -= Time.deltaTime;

                if (Parent.MainAnimationRemainingTime <= 0)
                {
                    if (Parent.StayTimeAfterFinished > 0)
                        Parent.SetNext<AfterAction>();
                    else
                        Parent.Complete();
                }
            }

            protected override void OnExit()
            {
                Parent.PlayActionExit?.Invoke();
            }
        }

        internal class AfterAction : Work<MonsterActionState>
        {
            private float remainingTime;

            protected override void OnEnter(params object[] _)
            {
                remainingTime = Parent.StayTimeAfterFinished;
                Parent.AfterActionEnter?.Invoke();
            }

            protected override void OnUpdate()
            {
                remainingTime -= Time.deltaTime;

                if (remainingTime <= 0)
                    Parent.Complete();
            }

            protected override void OnExit()
            {
                Parent.AfterActionExit?.Invoke();
            }
        }
    }
}
