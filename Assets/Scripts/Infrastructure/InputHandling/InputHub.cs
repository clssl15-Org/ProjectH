using System;
using System.Collections.Generic;
using System.Linq;
using BlackboxSystem;
using UnityEngine;

namespace Infrastructure
{
    public class InputHub : MonoBehaviour, IInputHub
    {
        private readonly Dictionary<IInputControllable, Action> _controllables = new();
        private bool _isBlocking = false;


        public void BlockAll()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("BlockAll");
            _isBlocking = true;

            foreach (var view in _controllables.Keys)
            {
                BlackboxHandle.Of(this).Exert(view, "Block");
                view.AllowInput = false;
            }
        }
        public void BlockExcept(params IInputControllable[] controllables)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("BlockExcept");
            _isBlocking = true;

            foreach (var controllable in _controllables.Keys)
            {
                if (!controllables.Contains(controllable))
                {
                    BlackboxHandle.Of(this).Exert(controllable, "Block");
                    controllable.AllowInput = false;
                }
            }
        }

        public void UnblockAll(bool delayFrame = true)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"UnblockAll (delay: {delayFrame})");
            _isBlocking = false;

            if (delayFrame)
            {
                IDisposable unblocker = null;
                unblocker = Loco.Subscribe(() =>
                {
                    unblocker.Dispose();

                    if (_isBlocking) return;
                    Unblock();
                });
            }
            else
            {
                Unblock();
            }


            void Unblock()
            {
                foreach (var controllable in _controllables.Keys)
                {
                    BlackboxHandle.Of(this).Exert(controllable, "Unblock");
                    controllable.AllowInput = true;
                }
            }
        }


        public void Register(IInputControllable controllable)
        {
            if (controllable == null)
                throw new ArgumentNullException(
                    nameof(controllable),
                    BlackboxHandle.Of(this).CrashExport(
                        $"[{nameof(InputHub)}] 등록할 {nameof(controllable)}은(는) null일 수 없습니다."));

            if (_controllables.ContainsKey(controllable))
                return;

            using var _ = BlackboxHandle.Of(this).ExertScope(controllable, "Register");

            _controllables[controllable] = () => Remove(controllable);
            controllable.Destroying += _controllables[controllable];
        }

        public void Remove(IInputControllable controllable)
        {
            if (!_controllables.ContainsKey(controllable))
            {
                using var __ = BlackboxHandle.Of(this)
                    .WriteScope($"{nameof(controllable)}을(를) 가지고 있지 않기 않기 때문에 Remove를 수행할 수 없습니다.");
                return;
            }

            using var _ = BlackboxHandle.Of(this).ExertScope(controllable, "Remove");

            controllable.Destroying -= _controllables[controllable];
            _controllables.Remove(controllable);
        }
    }
}
