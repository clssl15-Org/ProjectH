using System;
using MonsterActions;
using UniEngine.StateMachines.FSM;
using UnityEngine;

public partial class StagBeetle : Monster
{
    private class ThreePhasedAction : Work<MonsterActionController>
    {
        protected StagBeetle Owner => (StagBeetle)Parent.Owner;

        private string[] Actions;

        private Action beforePreAction;
        private Action beforeMainAction;
        private Func<float, bool> whileMainAction;
        private Action afterMainAction;

        private Action<ActionResult> callback;
        private bool isCompleted;


        // Content
        public ThreePhasedAction(
            string actionName,
            Func<string, string> getPreActionName,
            Func<string, string> getPostActionName,
            Action beforePreAction = null,
            Action beforeMainAction = null,
            Func<float, bool> whileMainAction = null,
            Action afterMainAction = null) : base(actionName)
        {
            Actions = new string[] { getPreActionName(actionName), actionName, getPostActionName(actionName) };
            this.beforePreAction = beforePreAction;
            this.beforeMainAction = beforeMainAction;
            this.whileMainAction = whileMainAction;
            this.afterMainAction = afterMainAction;

            AddChild(new SubAction(), true);
            AddChild(new MainAction());
        }

        protected override void OnEnter(params object[] inputs)
        {
            callback = (Action<ActionResult>)inputs[0];
            isCompleted = false;
        }

        protected void Complete()
        {
            isCompleted = true;
            Exit();
        }

        protected override void OnExit()
        {
            callback?.Invoke(new(isCompleted
                ? ActionResult.ResultType.Success
                : ActionResult.ResultType.Interrupted));

            callback = null;
        }



        // Substates
        private class SubAction : Work<ThreePhasedAction>
        {
            private StagBeetle Owner => Parent.Owner;
            private Animator Animator => Parent.Owner.Animator;

            private bool isPreAction;
            private float remainingTime;


            protected override void OnEnter(params object[] inputs)
            {
                isPreAction = inputs == null || inputs.Length == 0 || (bool)inputs[0];

                if (!Owner.Animator.TryFindClip(Parent.Actions[isPreAction ? 0 : 2], out var clip))
                    throw new ArgumentException(Owner.Ctx($"애니메이터가 동작 {MonsterAction.Attack.ToString()}을(를) 가지고 있지 않습니다."));

                remainingTime = clip.length;
                Owner.Animator.Play(clip.name);

                if (isPreAction)
                    Parent.beforePreAction?.Invoke();
            }

            protected override void OnUpdate()
            {
                if ((remainingTime -= Time.deltaTime) <= 0)
                {
                    if (isPreAction)
                        Parent.SetNext<MainAction>();
                    else
                        Parent.Complete();
                }
            }
        }

        private class MainAction : Work<ThreePhasedAction>
        {
            private StagBeetle Owner => Parent.Owner;
            private Animator Animator => Parent.Owner.Animator;

            private float playtime;


            protected override void OnEnter(params object[] _)
            {
                if (!Owner.Animator.TryFindClip(Parent.Actions[1], out var clip))
                    throw new ArgumentException(Owner.Ctx($"애니메이터가 동작 {MonsterAction.Attack.ToString()}을(를) 가지고 있지 않습니다."));

                playtime = 0;
                Owner.Animator.Play(clip.name);

                Parent.beforeMainAction?.Invoke();
            }

            protected override void OnUpdate()
            {
                if (!Parent?.whileMainAction(playtime += Time.deltaTime) ?? false)
                    Parent.SetNextWith<SubAction>(false);
            }

            protected override void OnExit()
            {
                Parent.afterMainAction?.Invoke();
            }
        }
    }
}
