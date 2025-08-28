using System;
using UniEngine.StateMachines.FSM;
using UnityEngine;

namespace MonsterActions
{
    internal class MonsteActionState : Work<MonsterActionController>
    {
        // Front
        public Monster Owner => Parent.Owner;

        public float? Playtime { get; protected set; } = null;
        protected float? remainingTime;

        private Action<ActionResult> callback;
        protected float stayTimeAfterFinished;
        protected bool isCompleted;

        protected Action PlayActionEnter, PlayActionExit;
        protected Action AfterActionEnter, AfterActionExit;


        // Content
        public MonsteActionState(MonsterAction monsterAction) : this(monsterAction.ToString()) { }
        public MonsteActionState(string name) : base(name)
        {
            AddChild(new PlayAction(), true);
            AddChild(new AfterAction());
        }

        protected override void OnEnter(params object[] inputs)
        {
            if (!Owner.Animator.TryFindClip(Name, out var clip))
                throw new ArgumentException(Owner.Ctx($"애니메이터가 동작 {MonsterAction.Attack.ToString()}을(를) 가지고 있지 않습니다."));

            SetInputs(inputs);
            remainingTime = Playtime ?? (!clip.isLooping ? clip.length : null);

            isCompleted = false;
            Owner.Animator.Play(clip.name);
        }
        protected void SetInputs(params object[] inputs)
        {
            callback = (Action<ActionResult>)inputs[0];
            Playtime = (float?)inputs[1];
            stayTimeAfterFinished = (float)inputs[2];
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
            protected override void OnEnter(params object[] inputs)
            {
                Parent.PlayActionEnter?.Invoke();
            }

            protected override void OnUpdate()
            {
                if (!Parent.remainingTime.HasValue)
                    return;


                Parent.remainingTime -= Time.deltaTime;

                if (Parent.remainingTime < 0)
                {
                    if (Parent.stayTimeAfterFinished > 0)
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
                remainingTime = Parent.stayTimeAfterFinished;
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
