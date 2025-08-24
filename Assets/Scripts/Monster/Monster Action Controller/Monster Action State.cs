using System;
using UnityEngine;
using UniEngine.StateMachines.FSM;

public abstract partial class Monster
{
    public partial class MonsterActionController : Work
    {
        private class MonsteActionState : Work<MonsterActionController>
        {
            // Front
            public Monster Owner => Parent.Owner;
            public float? Playtime { get; set; } = null;

            private Action callback;
            private float callbackToleranceTime => Parent.CallbackToleranceTime;

            private float? remainingTime;


            // Content
            public MonsteActionState(string name) : base(name) { }

            protected override void OnEnter(params object[] inputs)
            {
                callback = (Action)inputs[1];
                Playtime = (float)inputs[0];

                Owner.Animator.Play(GetType().Name);
                var clip = Owner.Animator.GetCurrentAnimatorClipInfo(0)[0].clip;

                remainingTime = Playtime ?? (!clip.isLooping ? clip.length : null);
            }

            protected override void OnUpdate()
            {
                if (!remainingTime.HasValue)
                    return;

                remainingTime -= Time.deltaTime;

                if (remainingTime < callbackToleranceTime)
                    Exit();
            }

            protected override void OnExit()
            {
                callback?.Invoke();
                callback = null;
            }
        }
    }
}
