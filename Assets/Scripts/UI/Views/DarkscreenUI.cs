using System;
using Infrastructure;
using UnityEngine;

namespace UI
{
    public class DarkscreenUI : MonoBehaviour, IEnablableView
    {
        public event Action Enabling;
        public event Action Enabled;
        public event Action Disabling;
        public event Action Disabled;

        Action IEnablable.OnEnabling => Enabling;
        Action IEnablable.OnEnabled => Enabled;
        Action IEnablable.OnDisabling => Disabling;
        Action IEnablable.OnDisabled => Disabled;

        [SerializeField] private Animation _animation;
        private EnableWithAnimation _enabler;
        private bool _initialized = false;


        private void EnsureInitialization()
        {
            if (_initialized) return;
            _initialized = true;

            _enabler = new EnableWithAnimation(_animation)
                .InitializeWithIEnablable(this);
        }
        private void Awake() => EnsureInitialization();

        public void Enable()
        {
            EnsureInitialization();
            _enabler.Enable();
        }
        public void Disable()
        {
            EnsureInitialization();
            _enabler.Disable();
        }

        public void SetToEnabled()
        {
            EnsureInitialization();
            _enabler.SetToEnabled();
        }
        public void SetToDisabled()
        {
            EnsureInitialization();
            _enabler.SetToDisabled();
        }
    }
}
