using Infrastructure;
using UnityEngine;

namespace Game.Stage
{
    public class RecordUnlockedSceneChangeFallback : MonoBehaviour, IInjectable<GameServices>
    {
        private const float DefaultPlaybackDuration = 8.5f;

        [SerializeField] private RecordUnlockedStageManager _stageManager;
        [SerializeField, Min(0f)] private float _extraDelay = 1f;

        private GameServices _gameServices;
        private float _elapsedTime;
        private bool _fallbackRequested;

        void IInjectable<GameServices>.Inject(GameServices gameServices)
            => _gameServices = gameServices;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            if (_fallbackRequested)
                return;

            ResolveReferences();

            if (_stageManager && _stageManager.HasRequestedSceneTransition)
            {
                enabled = false;
                return;
            }

            _elapsedTime += Time.unscaledDeltaTime;

            if (_elapsedTime < GetFallbackDelay())
                return;

            RequestFallbackSceneTransition();
        }

        private void ResolveReferences()
        {
            if (!_stageManager)
                _stageManager = FindAnyObjectByType<RecordUnlockedStageManager>(FindObjectsInactive.Include);

            if (!_gameServices)
                _gameServices = FindAnyObjectByType<GameServices>(FindObjectsInactive.Include);
        }

        private float GetFallbackDelay()
        {
            float playbackDuration = _stageManager
                ? _stageManager.ExpectedPlaybackDuration
                : DefaultPlaybackDuration;

            return playbackDuration + _extraDelay;
        }

        private void RequestFallbackSceneTransition()
        {
            _fallbackRequested = true;

            if (_stageManager && _stageManager.RequestSceneTransition(this, _gameServices))
            {
                enabled = false;
                return;
            }

            Debug.LogError(
                $"[{nameof(RecordUnlockedSceneChangeFallback)}] 기록 해금 씬 폴백 전환을 실행하지 못했습니다.",
                this);
        }
    }
}
