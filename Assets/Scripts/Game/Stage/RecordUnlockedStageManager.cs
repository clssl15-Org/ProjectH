using System.Collections;
using Infrastructure;
using TMPro;
using UnityEngine;

namespace Game.Stage
{
    public class RecordUnlockedStageManager : MonoBehaviour, IInjectable<GameServices>
    {
        [SerializeField] private TextMeshProUGUI[] _texts;

        [Header("Fade Settings")]
        [SerializeField, Min(0)] private float _fadeDuration = 1.5f;
        [SerializeField, Min(0)] private float _displayDuration = 1f;

        private GameServices _gameServices;

        void IInjectable<GameServices>.Inject(GameServices gameServices)
            => _gameServices = gameServices;

        private void Start()
        {
            foreach (var text in _texts)
            {
                text.gameObject.SetActive(false);
                SetTextAlpha(text, 0f);
            }

            StartCoroutine(ShowTextsSequentiallyRoutine());
        }

        private IEnumerator ShowTextsSequentiallyRoutine()
        {
            yield return new WaitForSeconds(_displayDuration);

            foreach (var text in _texts)
            {
                text.gameObject.SetActive(true);
                yield return StartCoroutine(FadeRoutine(text, 1f, _fadeDuration));

                yield return new WaitForSeconds(_displayDuration);

                yield return StartCoroutine(FadeRoutine(text, 0f, _fadeDuration));
                text.gameObject.SetActive(false);
            }

            if (_gameServices.IsGameCleared)
                _gameServices.ChangeScene("Title", this, preservePlayerProgress: false);
            else
                _gameServices.ToFirstScene(this);
        }

        private IEnumerator FadeRoutine(TextMeshProUGUI text, float targetAlpha, float duration)
        {
            float startAlpha = text.color.a;
            float time = 0f;

            while (time < duration)
            {
                time += Time.deltaTime;
                float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
                SetTextAlpha(text, currentAlpha);

                yield return null;
            }

            SetTextAlpha(text, targetAlpha);
        }

        private void SetTextAlpha(TextMeshProUGUI text, float alpha)
        {
            Color color = text.color;
            color.a = alpha;
            text.color = color;
        }
    }
}
