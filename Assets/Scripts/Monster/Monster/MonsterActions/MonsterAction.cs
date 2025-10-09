using System;
using System.Collections.Generic;
using System.Linq;
using UniEngine.StateMachines.FSM;
using UnityEngine;
using static MonsterActions.MonsterActionComponent;

namespace MonsterActions
{
    public sealed class MonsterAction : Work
    {
        // Front
        public Monster Owner
        {
            get
            {
                if (Parent is not MonsterActionController parent)
                    throw new InvalidOperationException(
                        Ctx($"{nameof(Parent)}은(는) {nameof(MonsterActionController)} 형식이어야 하지만 '{Parent?.GetType().Name ?? "null"}'이(가) 감지되었습니다. " +
                        $"Owner을 반환할 수 없습니다."));

                return parent.Owner;
            }
        }


        // Internal
        private List<MonsterActionComponent> _components = new();
        private MonsterActionPlayInfo _playInfo;

        private InterruptType _reason;
        public float ElapsedTime { get; private set; }


        // Content
        public MonsterAction(MonsterActionType monsterAction) : this(monsterAction.ToString()) { }
        public MonsterAction(string name) : base(name) { }

        public MonsterAction AddComponent(MonsterActionComponent component)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component),
                    Ctx($"{nameof(component)}은(는) null일 수 없습니다."));

            _components.Add(component);
            component.SetParent(this);

            return this;
        }

        public MonsterAction AddAnimationComponent(
            PlayInfo playInfo = null,
            string trigger = null,
            float delayBeforePlay = 0f,
            float delayAfterPlay = 0f)
        {
            AddComponent(new PlayAnimation(playInfo != null
                ? playInfo with { TriggerName = trigger ?? playInfo.TriggerName }
                : new PlayInfo(Name, trigger))
                {
                    DelayBeforePlay = delayBeforePlay,
                    DelayAfterPlay = delayAfterPlay
                });

            return this;
        }


        public record MonsterActionPlayInfo
        (
            Action<ActionResult> Callback,
            float? PlayTime = null,
            object[] Inputs = null
        );

        protected override void OnEnter(params object[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs),
                    Ctx($"{nameof(inputs)}은(는) null일 수 없습니다."));

            if (inputs.Length != 1)
                throw new ArgumentException(
                    Ctx($"{nameof(inputs)}은(는) 1의 길이를 가져야 하지만 길이 '{inputs.Length}'을 가진 인자가 입력되었습니다."), nameof(inputs));

            if (inputs[0] is not MonsterActionPlayInfo playInfo)
                throw new ArgumentException(
                    Ctx($"{nameof(inputs)}은(는) {nameof(MonsterActionPlayInfo)} 형식이어야 하지만 '{inputs[0]?.GetType().Name ?? "null"}' 형식이 입력되었습니다."));


            _playInfo = playInfo;
            _reason = InterruptType.None;
            ElapsedTime = 0f;

            try
            {
                for (int i = 0; i < _components.Count; i++)
                {
                    if (playInfo.Inputs != null && i < playInfo.Inputs.Length)
                        _components[i].Enter(playInfo.Inputs[i]);
                    else
                        _components[i].Enter();
                }
            }
            catch
            {
                ExitWith(InterruptType.Error);
                throw;
            }
        }

        protected override void OnUpdate()
        {
            ElapsedTime += Time.deltaTime;

            if (_playInfo.PlayTime.HasValue)
            {
                if (ElapsedTime >= _playInfo.PlayTime)
                {
                    ElapsedTime = _playInfo.PlayTime.Value;
                    ExitWith(InterruptType.Timeover);
                    return;
                }
            }

            try
            {
                var components = _components.Where(c => c.Active);

                if (!_playInfo.PlayTime.HasValue && !components.Any())
                {
                    ExitWith(InterruptType.Completed);
                    return;
                }

                foreach (var component in components)
                    component.Update(ElapsedTime);
            }
            catch
            {
                ExitWith(InterruptType.Error);
                throw;
            }
        }

        protected override void OnExit()
        {
            if (_reason == InterruptType.None)
            {
                StopComponents(InterruptType.Interrupted);
                _playInfo.Callback?.Invoke(new ActionResult(ActionResult.ResultType.Interrupted));
                return;
            }

            _playInfo.Callback?.Invoke(new ActionResult(_reason.ToResultType()));
            _playInfo = null;
        }

        private void ExitWith(InterruptType reason)
        {
            _reason = reason;
            StopComponents(reason);

            Exit();
        }

        private void StopComponents(InterruptType interruptType)
        {
            foreach (var component in _components)
                component.Interrupt(interruptType);
        }
    }
}
