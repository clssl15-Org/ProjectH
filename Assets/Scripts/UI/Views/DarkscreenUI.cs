using System;
using System.Collections;
using Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
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

        [Header("Alpha")]
        [SerializeField, Range(0f, 1f)] private float _enabledAlpha = 0.5f;
        [SerializeField, Range(0f, 1f)] private float _closedAlpha = 1f;

        [Header("Durations")]
        [SerializeField, Min(0f)] private float _enableDuration = 0.15f;
        [SerializeField, Min(0f)] private float _disableDuration = 0.15f;
        [SerializeField, Min(0f)] private float _closeDuration = 0.15f;

        private CanvasGroup _canvasGroup;

        private MonoBehaviour _recentTarget;
        private Action _clickCallback;

        private Coroutine _transitionCoroutine;
        private Coroutine _enableForCoroutine;

        private bool _isInitialized = false;
        private bool _isClosed = false;
        private bool _isEnabled = false;

        private void Awake() => EnsureInitialization();
        private void EnsureInitialization()
        {
            if (_isInitialized)
                return;

            _isInitialized = true;

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            var active = gameObject.activeSelf;
            _isEnabled = active;

            _canvasGroup.alpha = active ? _enabledAlpha : 0f;
            _canvasGroup.blocksRaycasts = active;
            _canvasGroup.interactable = active;
        }

        void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
        {
            if (_isClosed)
                return;

            if (eventData.button == PointerEventData.InputButton.Left)
                _clickCallback?.Invoke();
        }

        public void EnableFor(MonoBehaviour target, Action clickCallback = null)
        {
            if (_isClosed)
                return;

            Enable();
            _clickCallback = clickCallback;

            if (!target)
                return;

            _recentTarget = target;
            EnableInternal(target);

            if (_enableForCoroutine != null)
                StopCoroutine(_enableForCoroutine);

            _enableForCoroutine = StartCoroutine(EnableForNextFrame(target));
        }

        public void CloseScreen(Action callback)
        {
            EnsureInitialization();

            if (_isClosed)
                return;

            _isClosed = true;
            _clickCallback = null;

            gameObject.SetActive(true);

            StartTransition(
                _closedAlpha,
                _closeDuration,
                onStart: () =>
                {
                    _canvasGroup.blocksRaycasts = true;
                    _canvasGroup.interactable = true;
                },
                onComplete: callback);
        }
        public void OpenScreen(Action callback = null)
        {
            EnsureInitialization();

            _isClosed = false;
            _canvasGroup.alpha = 1f;
            gameObject.SetActive(true);

            StartTransition(
                0f,
                _closeDuration,
                onComplete: () =>
                {
                    callback?.Invoke();
                    SetToDisabled();
                });
        }

        public void Enable()
        {
            if (_isClosed)
                return;

            EnsureInitialization();

            if (_isEnabled && gameObject.activeSelf)
                return;

            _isEnabled = true;
            gameObject.SetActive(true);

            StartTransition(
                _enabledAlpha,
                _enableDuration,
                onStart: () =>
                {
                    Enabling?.Invoke();
                    _canvasGroup.blocksRaycasts = true;
                    _canvasGroup.interactable = true;
                },
                onComplete: () =>
                {
                    Enabled?.Invoke();
                });
        }

        public void Disable()
        {
            EnsureInitialization();

            if (!_isEnabled && !gameObject.activeSelf)
                return;

            _isEnabled = false;
            _isClosed = false;
            _clickCallback = null;

            if (_enableForCoroutine != null)
            {
                StopCoroutine(_enableForCoroutine);
                _enableForCoroutine = null;
            }

            gameObject.SetActive(true);

            StartTransition(
                0f,
                _disableDuration,
                onStart: () =>
                {
                    Disabling?.Invoke();
                    _canvasGroup.blocksRaycasts = false;
                    _canvasGroup.interactable = false;
                },
                onComplete: () =>
                {
                    gameObject.SetActive(false);
                    Disabled?.Invoke();
                });
        }

        public void SetToEnabled()
        {
            if (_isClosed)
                return;

            EnsureInitialization();

            StopTransition();

            _isEnabled = true;
            gameObject.SetActive(true);

            _canvasGroup.alpha = _enabledAlpha;
            _canvasGroup.blocksRaycasts = true;
            _canvasGroup.interactable = true;

            Enabled?.Invoke();
        }

        public void SetToDisabled()
        {
            EnsureInitialization();

            StopTransition();

            _isEnabled = false;
            _isClosed = false;
            _clickCallback = null;

            if (_enableForCoroutine != null)
            {
                StopCoroutine(_enableForCoroutine);
                _enableForCoroutine = null;
            }

            _canvasGroup.alpha = 0f;
            _canvasGroup.blocksRaycasts = false;
            _canvasGroup.interactable = false;
            gameObject.SetActive(false);

            Disabled?.Invoke();
        }

        private IEnumerator EnableForNextFrame(MonoBehaviour target)
        {
            yield return null;

            if (target && _recentTarget == target)
                EnableInternal(target);

            _enableForCoroutine = null;
        }

        private void EnableInternal(MonoBehaviour target)
        {
            var targetRoot = GetRootUIObject(target.transform);
            int targetIndex = targetRoot.GetSiblingIndex();
            int myIndex = transform.GetSiblingIndex();

            if (transform.parent == targetRoot.parent && myIndex == targetIndex - 1)
                return;

            transform.SetSiblingIndex(targetIndex);
        }

        private Transform GetRootUIObject(Transform current)
        {
            if (current.parent == null)
                return current;

            if (current.parent.GetComponent<Canvas>() != null)
                return current;

            return GetRootUIObject(current.parent);
        }

        private void StartTransition(
            float targetAlpha,
            float duration,
            Action onStart = null,
            Action onComplete = null)
        {
            StopTransition();
            _transitionCoroutine = StartCoroutine(
                TransitionCoroutine(targetAlpha, duration, onStart, onComplete));
        }

        private void StopTransition()
        {
            if (_transitionCoroutine == null)
                return;

            StopCoroutine(_transitionCoroutine);
            _transitionCoroutine = null;
        }

        private IEnumerator TransitionCoroutine(
            float targetAlpha,
            float duration,
            Action onStart,
            Action onComplete)
        {
            onStart?.Invoke();

            float startAlpha = _canvasGroup.alpha;

            if (Mathf.Approximately(duration, 0f))
            {
                _canvasGroup.alpha = targetAlpha;
                _transitionCoroutine = null;
                onComplete?.Invoke();
                yield break;
            }

            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                _canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
                yield return null;
            }

            _canvasGroup.alpha = targetAlpha;
            _transitionCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
