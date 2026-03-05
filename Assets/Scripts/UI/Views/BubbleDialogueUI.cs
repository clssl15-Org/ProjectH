using System;
using BlackboxSystem;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BubbleDialogueUI : MonoBehaviour
    {
        private RectTransform _transform;
        [SerializeField] private TextMeshProUGUI _textUI;

        private Func<Vector2> _getPosition;
        private Vector2 _offset;

        public interface IContainer
        {
            string Text { get; }
            Func<Vector2> GetPosition { get; }
            Vector2 Offset { get; }
        }

        private void Awake()
        {
            _transform = GetComponent<RectTransform>();
        }

        public Vector2 GetPreferredValues(string text, float maxWidth)
        {
            var originalSize = _textUI.GetPreferredValues(text);

            if (originalSize.x <= maxWidth)
                return originalSize;

            var height = _textUI.GetPreferredValues(text, maxWidth, float.PositiveInfinity).y;
            return new Vector2(maxWidth, height);
        }

        public Vector2 GetPreferredTextSize(string text)
        {
            return _textUI.GetPreferredValues(text);
        }

        public void SetPanelSize(Vector2 size)
        {
            if (!_transform) _transform = GetComponent<RectTransform>();
            _transform.sizeDelta = size;
        }

        public void Show(IContainer container) => Show(container.Text, container.GetPosition, container.Offset);
        public void Show(string text, Func<Vector2> getPosition, Vector2 offset)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Show: {text}");

            _textUI.text = text;
            _getPosition = getPosition;
            _offset = offset;

            FixedUpdate();
            gameObject.SetActive(true);
        }

        private void FixedUpdate()
        {
            if (!_transform)
                _transform = GetComponent<RectTransform>();

            _transform.anchoredPosition =
                (_getPosition?.Invoke() ?? Vector2.zero) + _offset;
        }

        public void Hide()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Hide");

            _textUI.text = string.Empty;
            _getPosition = null;

            gameObject.SetActive(false);
        }
    }
}