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
        protected float? RemainingTime { get; private set; }

        private Action<ActionResult> callback;
        private float stayTimeAfterFinished;
        private bool isCompleted;



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


            callback = (Action<ActionResult>)inputs[0];
            Playtime = (float?)inputs[1];
            stayTimeAfterFinished = (float)inputs[2];

            RemainingTime = Playtime ?? (!clip.isLooping ? clip.length : null);

            isCompleted = false;
            Owner.Animator.Play(clip.name);
        }

        private void Complete()
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
        private class PlayAction : Work<MonsteActionState>
        {
            protected override void OnUpdate()
            {
                if (!Parent.RemainingTime.HasValue)
                    return;


                Parent.RemainingTime -= Time.deltaTime;

                if (Parent.RemainingTime < 0)
                {
                    if (Parent.stayTimeAfterFinished > 0)
                        Parent.SetNext<AfterAction>();
                    else
                        Parent.Complete();
                }
            }
        }

        private class AfterAction : Work<MonsteActionState>
        {
            private float remainingTime;

            protected override void OnEnter(params object[] _)
            {
                remainingTime = Parent.stayTimeAfterFinished;
            }

            protected override void OnUpdate()
            {
                remainingTime -= Time.deltaTime;

                if (remainingTime <= 0)
                    Parent.Complete();
            }
        }
    }
}
