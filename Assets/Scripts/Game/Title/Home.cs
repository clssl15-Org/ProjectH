using System;
using Infrastructure;
using UnityEngine;

namespace Game.Title
{
    public class Home : MonoBehaviour, IEnablable
    {
        public event Action Enabling;
        public event Action Disabling;

        Action IEnablable.OnEnabling => Enabling;
        Action IEnablable.OnDisabling => Disabling;
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabled => null;

        [SerializeField] private Animation _animation;
        private EnableWithAnimation _enabler;

        private void Awake()
        {
            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);
        }

        private void Update()
        {
            if (_enabler.IsEnabled
                && Input.anyKeyDown
                && !Input.GetKey(KeyCode.Escape))
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
