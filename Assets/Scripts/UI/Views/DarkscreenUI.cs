using System;
using Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    public class DarkscreenUI : MonoBehaviour, IPointerClickHandler, IEnablable
    {
        public event Action Enabling;
        public event Action Enabled;
        public event Action Disabling;
        public event Action Disabled;

        #region Interfaces
        Action IEnablable.OnEnabling => Enabling;
        Action IEnablable.OnEnabled => Enabled;
        Action IEnablable.OnDisabling => Disabling;
        Action IEnablable.OnDisabled => () =>
        {
            Disabled?.Invoke();
            _clickCallback = null;
        };
        #endregion

        [SerializeField] private Animation _animation;
        private EnableWithAnimation _enabler;

        private MonoBehaviour _recentTarget;
        private Action _clickCallback;

        private bool _initialized = false;


        private void EnsureInitialization()
        {
            if (_initialized) return;
            _initialized = true;

            _enabler = new EnableWithAnimation(_animation, gameObject.activeSelf)
                .InitializeWithIEnablable(this);
        }
        private void Awake() => EnsureInitialization();

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                if (_clickCallback != null)
                {
                    _clickCallback.Invoke();
                    _clickCallback = null;
                }
            }
        }

        public void EnableFor(MonoBehaviour target, Action clickCallback = null)
        {
            Enable();
            _clickCallback = clickCallback;

            if (target)
            {
                _recentTarget = target;
                EnableInternal(target);

                IDisposable token = null;
                token = Loco.Subscribe(() =>
                {
                    if (target && _recentTarget == target)
                        EnableInternal(target);

                    token.Dispose();
                });
            }

            void EnableInternal(MonoBehaviour target)
            {
                var targetRoot = GetRootUIObject(target.transform);
                Transform GetRootUIObject(Transform current)
                {
                    if (current.parent == null) return current;
                    if (current.parent.GetComponent<Canvas>() != null) return current;

                    return GetRootUIObject(current.parent);
                }

                var targetIndex = targetRoot.GetSiblingIndex();
                var myIndex = transform.GetSiblingIndex();

                if (transform.parent == targetRoot.parent && myIndex == targetIndex - 1)
                    return;

                transform.SetSiblingIndex(targetIndex);
            }
        }
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

        private void OnDestroy()
        {
            _enabler?.Dispose();
        }
    }
}
