using System;
using UniEngine.StateMachines.FSM;
using UnityEngine;

namespace MonsterActions
{
    internal class MonsteActionState : Work<MonsterActionController>
    {
        // Front
        public Monster Owner => Parent.Owner;

        public float? Playtime { get; private set; } = null;
        protected float? MainAnimationRemainingTime { get; set; }

        private Action<ActionResult> callback;
        protected float StayTimeAfterFinished { get; private set; }
        protected bool isCompleted;

        protected Action PlayActionEnter, PlayActionExit;
        protected Action AfterActionEnter, AfterActionExit;


        // Content
        public MonsteActionState(MonsterAction monsterAction) : this(monsterAction.ToString()) { }
        public MonsteActionState(string name) : base(name) => Initialize();
        protected virtual void Initialize()
        {
            AddChild(new PlayAction(), true);
            AddChild(new AfterAction());
        }

        protected override void OnEnter(params object[] inputs)
        {
            if (!Owner.Animator.TryFindClip(Name, out var clip))
                throw new ArgumentException(Owner.Ctx($"애니메이터가 동작 {MonsterAction.Attack.ToString()}을(를) 가지고 있지 않습니다."));

            SetInputs(inputs);

            MainAnimationRemainingTime = Playtime.HasValue
                ? (Playtime.Value >= 0 ? Playtime.Value : null)
                : (!clip.isLooping ? clip.length : null);

            isCompleted = false;
            Owner.Animator.Play(clip.name);
        }
        protected void SetInputs(params object[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs), Ctx("Inputs는 null일 수 없습니다."));

            if (inputs.Length < 3)
                throw new ArgumentException(Ctx(
                    $"Inputs는 3개 이상의 인자를 가져야 합니다. 현재 Inputs는 '{inputs.Length}'개의 인자를 가지고 있습니다."), nameof(inputs));

            callback = (Action<ActionResult>)inputs[0];
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
            var callback = this.callback;
            this.callback = null;

            callback?.Invoke(new(isCompleted
                ? ActionResult.ResultType.Success
                : ActionResult.ResultType.Interrupted));
        }



        // Substates
        internal class PlayAction : Work<MonsteActionState>
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

        internal class AfterAction : Work<MonsteActionState>
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
