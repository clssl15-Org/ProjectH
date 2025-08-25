using System;
using UniEngine.StateMachines.FSM;
using UnityEngine;

namespace MonsterActions
{
    internal class MonsteActionState : Work<MonsterActionController>
    {
        // Front
        public Monster Owner => Parent.Owner;
        public float? Playtime { get; set; } = null;

        protected Action<ActionResult> callback;
        public virtual float CallbackToleranceTime
        {
            get => _callbackToleranceTime ??= Parent.DefaultCallbackToleranceTime;
            set => _callbackToleranceTime = value;
        }

        private float? _callbackToleranceTime;
        protected float? RemainingTime { get; private set; }


        // Content
        public MonsteActionState(string name) : base(name) { }

        protected override void OnEnter(params object[] inputs)
        {
            var clip = Owner.Animator.FindClip(Name);

            if (clip == null) throw new ArgumentException(
                Owner.Ctx($"애니메이터가 동작 {Name}을(를) 가지고 있지 않습니다."));

            callback = (Action<ActionResult>)inputs[0];
            Playtime = (float?)inputs[1];

            RemainingTime = Playtime ?? (clip.isLooping ? clip.length : null);
            Owner.Animator.Play(Name);
        }

        protected override void OnUpdate()
        {
            if (!RemainingTime.HasValue)
                return;

            RemainingTime -= Time.deltaTime;

            if (RemainingTime < CallbackToleranceTime)
                Exit();
        }

        protected override void OnExit()
        {
            callback?.Invoke(new(ActionResult.ResultType.Success));
            callback = null;
        }
    }
}
