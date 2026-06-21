using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPTypeHandler : MonoBehaviour
    {
        [SerializeField, Min(0)] private float _typingSpeed = 0.1f;

        public event Action TextTyped;
        public bool IsTotallyTyped => _typingCoroutine == null;

        private TextMeshProUGUI _textUI;
        private Coroutine _typingCoroutine;

        private bool _isAwake;

        private void Awake() => EnsureInitialization();
        private void EnsureInitialization()
        {
            if (_isAwake) return;
            _isAwake = true;

            _textUI = GetComponent<TextMeshProUGUI>();
        }

        /// <summary>
        /// 이 메서드는 gameObject가 Active 상태일 때만 실행할 수 있습니다.
        /// </summary>
        public void TypeDialogue(string textContent, bool showImmediately = false)
        {
            EnsureInitialization();

            if (_typingCoroutine != null) StopCoroutine(_typingCoroutine);
            if (showImmediately)
            {
                _typingCoroutine = null;
                _textUI.text = textContent;
                _textUI.maxVisibleCharacters = int.MaxValue;
                return;
            }

            _typingCoroutine = StartCoroutine(TypeRoutine(textContent));
        }

        private IEnumerator TypeRoutine(string textContent)
        {
            _textUI.text = textContent;
            _textUI.ForceMeshUpdate();

            var textInfo = _textUI.textInfo;
            var totalVisibleCharacterCount = textInfo.characterCount;

            if (totalVisibleCharacterCount == 0)
            {
                _textUI.maxVisibleCharacters = 0;
                _typingCoroutine = null;
                yield break;
            }

            int counter = 1;
            _textUI.maxVisibleCharacters = counter;

            if (IsVisibleCharacter(0, textInfo))
                TextTyped?.Invoke();

            while (counter < totalVisibleCharacterCount)
            {
                yield return new WaitForSeconds(_typingSpeed);

                counter++;
                _textUI.maxVisibleCharacters = counter;

                if (IsVisibleCharacter(counter - 1, textInfo))
                    TextTyped?.Invoke();
            }

            _typingCoroutine = null;

            bool IsVisibleCharacter(int index, TMP_TextInfo textInfo)
            {
                if (index >= textInfo.characterInfo.Length)
                    return false;

                var info = textInfo.characterInfo[index];
                return char.IsLetterOrDigit(info.character);
            }
        }

        public void SkipTyping()
        {
            EnsureInitialization();

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            _textUI.maxVisibleCharacters = int.MaxValue;
            TextTyped?.Invoke();
        }

        public void ClearText()
        {
            EnsureInitialization();

            if (_typingCoroutine != null)
            {
                StopCoroutine(_typingCoroutine);
                _typingCoroutine = null;
            }

            _textUI.text = string.Empty;
        }

        public Vector2 GetPreferredValues(string text)
        {
            EnsureInitialization();
            return _textUI.GetPreferredValues(text);
        }
        public Vector2 GetPreferredValues(string text, float maxWidth, float maxHeight)
        {
            EnsureInitialization();
            return _textUI.GetPreferredValues(text, maxWidth, maxHeight);
        }
    }
}