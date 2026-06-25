using System.Collections;
using Infrastructure;
using TMPro;
using UnityEngine;

namespace Game.Stage
{
    public class RecordUnlockedStageManager : MonoBehaviour, IInjectable<GameServices>
    {
        private const string TitleSceneName = "Title";

        [SerializeField] private TextMeshProUGUI[] _texts;

        [Header("Fade Settings")]
        [SerializeField, Min(0)] private float _fadeDuration = 1.5f;
        [SerializeField, Min(0)] private float _displayDuration = 1f;

        private GameServices _gameServices;
        private bool _hasRequestedSceneTransition;

        public float ExpectedPlaybackDuration =>
            _displayDuration + ((_texts?.Length ?? 0) * ((_fadeDuration * 2f) + _displayDuration));
        public bool HasRequestedSceneTransition => _hasRequestedSceneTransition;

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

            RequestSceneTransition(this);
        }

        public bool RequestSceneTransition(object context = null, GameServices overrideGameServices = null)
        {
            if (_hasRequestedSceneTransition)
                return false;

            GameServices targetGameServices = overrideGameServices ? overrideGameServices : _gameServices;
            if (!targetGameServices)
            {
                Debug.LogError(
                    $"[{nameof(RecordUnlockedStageManager)}] {nameof(GameServices)}가 없어 다음 씬으로 전환할 수 없습니다.",
                    this);
                return false;
            }

            _hasRequestedSceneTransition = true;

            if (targetGameServices.IsGameCleared)
                targetGameServices.ChangeScene(TitleSceneName, context ?? this, preservePlayerProgress: false);
            else
                targetGameServices.ToFirstScene(context ?? this);

            return true;
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
