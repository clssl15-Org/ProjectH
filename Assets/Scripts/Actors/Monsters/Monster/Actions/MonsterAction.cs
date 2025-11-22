using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure.StateMachines.Fsm;
using UnityEngine;

namespace Actors.Monsters.Actions
{
    internal sealed class MonsterAction : Work
    {
        // Front
        public record PlayOrder(params MonsterActionComponent[] Afters)
        {
            public bool HasDependency => Afters != null && Afters.Length > 0;

            public bool CanPlay(IList<MonsterActionComponent> completes) =>
                !HasDependency || Afters.All(after => completes.Any(c => c == after));
        }

        public IMonsterInternal Owner
        {
            get
            {
                if (Parent is not MonsterActionController parent)
                    throw new InvalidOperationException(
                        FormatLogMessage($"{nameof(Parent)}은(는) {nameof(MonsterActionController)} 형식이어야 하지만 '{Parent?.GetType().Name ?? "null"}'이(가) 감지되었습니다. " +
                        $"Owner을 반환할 수 없습니다."));

                return parent.Owner;
            }
        }

        public float ElapsedTime { get; private set; }


        // Internal
        private readonly Dictionary<MonsterActionComponent, (PlayOrder order, object input)> _components = new();
        private readonly List<MonsterActionComponent> _orderedComponents = new();
        private readonly List<MonsterActionComponent> _readyBuffer = new();

        private Action<ActionResult> _callback;

        private readonly List<MonsterActionComponent> _pendings = new();
        private readonly List<MonsterActionComponent> _runnings = new();
        private readonly List<MonsterActionComponent> _completes = new();

        private InterruptType _reason;


        // Content
        public MonsterAction(MonsterActionType monsterAction) : this(monsterAction.ToString()) { }
        public MonsterAction(string name) : base(name) { }

        public MonsterAction AddComponent(MonsterActionComponent component, PlayOrder after = null) => AddComponent(component, out _, after);
        public MonsterAction AddComponent(MonsterActionComponent component, out MonsterActionComponent self, PlayOrder after = null)
        {
            if (component == null)
                throw new ArgumentNullException(nameof(component),
                    FormatLogMessage($"{nameof(component)}은(는) null일 수 없습니다."));

            _components.Add(component, (after ?? new(), null));
            _orderedComponents.Add(component);
            component.SetParent(this);

            self = component;
            return this;
        }

        #region Tools
        public MonsterAction AddDelayComponent(float delayDuration = 0f, bool interruptAllOnDeactivate = false, PlayOrder after = null) =>
            AddComponent(new Delay(delayDuration, interruptAllOnDeactivate), after);
        public MonsterAction AddDelayComponent(float delayDuration, out MonsterActionComponent self, bool interruptAllOnDeactivate = false, PlayOrder after = null) =>
            AddComponent(new Delay(delayDuration, interruptAllOnDeactivate), out self, after);


        public MonsterAction AddAnimationComponent(
            string animName,
            string trigger = null,
            bool interruptAllOnDeactivate = false,
            float delayBeforePlay = 0f,
            float delayAfterPlay = 0f,
            PlayOrder after = null) =>
            AddAnimationComponent(new MonsterAnimationPlayInfo(animName), out _, trigger, interruptAllOnDeactivate, delayBeforePlay, delayAfterPlay, after);

        public MonsterAction AddAnimationComponent(
            string animName,
            out MonsterActionComponent self,
            string trigger = null,
            bool interruptAllOnDeactivate = false,
            float delayBeforePlay = 0f,
            float delayAfterPlay = 0f,
            PlayOrder after = null) =>
            AddAnimationComponent(new MonsterAnimationPlayInfo(animName), out self, trigger, interruptAllOnDeactivate, delayBeforePlay, delayAfterPlay, after);

        public MonsterAction AddAnimationComponent(
            MonsterAnimationPlayInfo animPlayInfo = null,
            string trigger = null,
            bool interruptAllOnDeactivate = false,
            float delayBeforePlay = 0f,
            float delayAfterPlay = 0f,
            PlayOrder after = null) =>
            AddAnimationComponent(animPlayInfo, out _, trigger, interruptAllOnDeactivate, delayBeforePlay, delayAfterPlay, after);

        public MonsterAction AddAnimationComponent(
            out MonsterActionComponent self,
            string trigger = null,
            bool interruptAllOnDeactivate = false,
            float delayBeforePlay = 0f,
            float delayAfterPlay = 0f,
            PlayOrder after = null) =>
            AddAnimationComponent((MonsterAnimationPlayInfo)null, out self, trigger, interruptAllOnDeactivate, delayBeforePlay, delayAfterPlay, after);


        public MonsterAction AddAnimationComponent(
            MonsterAnimationPlayInfo animPlayInfo,
            out MonsterActionComponent self,
            string trigger = null,
            bool interruptAllOnDeactivate = false,
            float delayBeforePlay = 0f,
            float delayAfterPlay = 0f,
            PlayOrder order = null)
        {
            AddComponent(new PlayAnimation(animPlayInfo != null
                ? animPlayInfo with { TriggerName = trigger ?? animPlayInfo.TriggerName }
                : new MonsterAnimationPlayInfo(Name, trigger))
            {
                InterruptAllOnDeactivate = interruptAllOnDeactivate,
                DelayBeforePlay = delayBeforePlay,
                DelayAfterPlay = delayAfterPlay
            }, out self, order);

            return this;
        }
        #endregion

        protected override void OnEnter(params object[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs),
                    FormatLogMessage($"{nameof(inputs)}은(는) null일 수 없습니다."));

            if (inputs.Length != 1)
                throw new ArgumentException(
                    FormatLogMessage($"{nameof(inputs)}은(는) 1의 길이를 가져야 하지만 길이 '{inputs.Length}'을 가진 인자가 입력되었습니다."), nameof(inputs));

            if (inputs[0] is not MonsterActionPlayInfo playInfo)
                throw new ArgumentException(
                    FormatLogMessage($"{nameof(inputs)}은(는) {nameof(MonsterActionPlayInfo)} 형식이어야 하지만 '{inputs[0]?.GetType().Name ?? "null"}' 형식이 입력되었습니다."));


            try
            {
                var absInputs = playInfo.Inputs ?? Array.Empty<object>();

                for (int i = 0; i < _orderedComponents.Count; i++)
                {
                    var component = _orderedComponents[i];

                    var input = i < absInputs.Length ? absInputs[i] : null;
                    _components[component] = (_components[component].order, input);
                }

            }
            catch
            {
                ExitWith(InterruptType.Error);
                throw;
            }

            _callback = playInfo.Callback;
            _reason = InterruptType.None;

            ElapsedTime = 0f;
            _pendings.Clear();
            _pendings.AddRange(_orderedComponents);

            _readyBuffer.Clear();
        }

        protected override void OnUpdate()
        {
            for (int i = _runnings.Count - 1; i >= 0; i--)
            {
                var component = _runnings[i];
                if (!component.Active)
                {
                    if (component.InterruptAllOnDeactivate)
                    {
                        ExitWith(InterruptType.Completed);
                        return;
                    }

                    _runnings.RemoveAt(i);
                    _completes.Add(component);
                }
            }

            if (_completes.Count >= _components.Count)
            {
                ExitWith(InterruptType.Completed);
                return;
            }

            ElapsedTime += Time.deltaTime;

            try
            {
                _readyBuffer.Clear();
                for (int i = 0; i < _pendings.Count; i++)
                {
                    var component = _pendings[i];

                    if (_components[component].order.CanPlay(_completes))
                        _readyBuffer.Add(component);
                }

                for (int i = 0; i < _readyBuffer.Count; i++)
                {
                    var component = _readyBuffer[i];

                    _pendings.Remove(component);
                    component.Enter(ElapsedTime, _components[component].input);
                    _runnings.Add(component);
                }

                for (int i = 0; i < _runnings.Count; i++)
                    _runnings[i].Update(ElapsedTime);
            }
            catch
            {
                ExitWith(InterruptType.Error);
                throw;
            }
        }

        protected override void OnExit()
        {
            try
            {
                if (_reason == InterruptType.None)
                {
                    StopAllComponents(InterruptType.Interrupted);
                    _callback?.Invoke(new ActionResult(ResultType.Interrupted));
                }
                else
                {
                    _callback?.Invoke(new ActionResult(_reason.ToResultType()));
                }
            }
            finally
            {
                _callback = null;
            }
        }

        private void ExitWith(InterruptType reason)
        {
            _reason = reason;
            StopAllComponents(reason);

            Exit();
        }

        private void StopAllComponents(InterruptType interruptType)
        {
            foreach (var (component, _) in _components)
                component.Interrupt(interruptType);

            _pendings.Clear();
            _runnings.Clear();
            _completes.Clear();
            _readyBuffer.Clear();
        }
    }
}
