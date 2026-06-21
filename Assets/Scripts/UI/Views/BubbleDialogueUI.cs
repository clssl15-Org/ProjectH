using System;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BubbleDialogueUI : MonoBehaviour, IDialogueUI
    {
        [SerializeField] private TMPTypeHandler _typeHandler;

        public event Action TextTyped;
        public bool IsTotallyTyped => _typeHandler.IsTotallyTyped;

        private RectTransform _transform;
        private Func<Vector2> _getPosition;
        private Vector2 _offset;

        public interface IContainer
        {
            string Text { get; }
            Func<Vector2> GetPosition { get; }
            Vector2 Offset { get; }
        }

        private void Awake() => _transform = GetComponent<RectTransform>();
        private void Start() => _typeHandler.TextTyped += () => TextTyped?.Invoke();

        public Vector2 GetPreferredValues(string text, float maxWidth)
        {
            var originalSize = _typeHandler.GetPreferredValues(text);

            if (originalSize.x <= maxWidth)
                return originalSize;

            var height = _typeHandler.GetPreferredValues(text, maxWidth, float.PositiveInfinity).y;
            return new Vector2(maxWidth, height);
        }

        public Vector2 GetPreferredTextSize(string text)
        {
            return _typeHandler.GetPreferredValues(text);
        }

        public void SetPanelSize(Vector2 size)
        {
            if (!_transform) _transform = GetComponent<RectTransform>();
            _transform.sizeDelta = size;
        }

        public void Show(IContainer container, bool showImmediately = false) =>
            Show(container.Text, container.GetPosition, container.Offset, showImmediately);
        public void Show(string text, Func<Vector2> getPosition, Vector2 offset, bool showImmediately = false)
        {
            _getPosition = getPosition;
            _offset = offset;

            FixedUpdate();
            gameObject.SetActive(true);

            _typeHandler.TypeDialogue(text, showImmediately);
        }

        public void SkipTyping() => _typeHandler.SkipTyping();

        private void FixedUpdate()
        {
            if (!_transform)
                _transform = GetComponent<RectTransform>();

            _transform.anchoredPosition = (_getPosition?.Invoke() ?? Vector2.zero) + _offset;
        }

        public void Hide()
        {

            _typeHandler.ClearText();
            _getPosition = null;

            gameObject.SetActive(false);
        }
    }
}
