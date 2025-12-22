using System;
using Infrastructure;
using UnityEngine;

namespace Game.Title
{
    public class Home : MonoBehaviour, IEnablable
    {
        public event Action Enabling;
        public event Action Disabling;

        Action IEnablable.Enabling => Enabling;
        Action IEnablable.Disabling => Disabling;
        Action IEnablable.Enabled => null;
        Action IEnablable.Disabled => null;

        [SerializeField] private Animation _animation;
        private EnableWithAnimation _enabler;

        private void Awake()
        {
            _enabler = new EnableWithAnimation(_animation)
                .InitializeWithIEnablable(this);
        }

        private void Update()
        {
            if (_enabler.Enabled && Input.anyKeyDown)
                Disable();
        }

        public void Enable() => _enabler.Enable();
        public void Disable() => _enabler.Disable();
        public void SetToEnabled() => _enabler.SetToEnabled();
        public void SetToDisabled() => _enabler.SetToDisabled();

        private void OnDestroy()
        {
            _enabler.Dispose();
            _enabler = null;
        }
    }
}
