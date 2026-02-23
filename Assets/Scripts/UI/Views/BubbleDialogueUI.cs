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


        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");
            _transform = GetComponent<RectTransform>();
        }

        public void Show(string text, Func<Vector2> getPosition, Vector2 offset)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Show: {text}");

            _textUI.text = text;
            _getPosition = getPosition;
            _offset = offset;
        }

        private void Update()
        {
            _transform.anchoredPosition =
                _getPosition?.Invoke() ?? Vector2.zero + _offset;
        }

        public void Hide()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Hide");

            _textUI.text = string.Empty;
            _getPosition = null;
        }

        private void OnDestroy()
        {
            _getPosition = null;
        }
    }
}
