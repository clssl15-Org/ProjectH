using UnityEngine;
using Infrastructure;
using UI;

namespace Game.Stage
{
    // 보스 클리어 후 진행 연출이 막혔을 때 뒤늦게 복구하는 안전장치입니다.
    public class BossClearObjectFallback : MonoBehaviour, IInjectable<GameServices>
    {
        private enum FallbackMode
        {
            ActivateClearObjects,
            ChangeToEndingScene,
        }

        [SerializeField] private FallbackMode _fallbackMode = FallbackMode.ActivateClearObjects;

        // 비워 두면 같은 씬 안에서 자동으로 찾습니다.
        [SerializeField] private SingleBossStageManager _stageManager;
        [SerializeField] private FinalBossStageManager _finalBossStageManager;
        [SerializeField] private GameObject _clearObjectsRoot;
        [SerializeField] private GameObject _boxObject;
        [SerializeField] private GameObject _portalObject;
        [SerializeField] private string _endingSceneName = "EndingScene";
        [SerializeField, Min(0f)] private float _fallbackDelay = 60f;
        [SerializeField] private bool _activateBox = true;
        [SerializeField] private bool _activatePortal = true;
        [SerializeField] private bool _useUnscaledTime = true;

        private bool _timerStarted;
        private float _elapsedTime;
        private GameServices _gameServices;

        void IInjectable<GameServices>.Inject(GameServices gameServices) => _gameServices = gameServices;

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            ResolveReferences();

            // 보스가 죽기 전에는 타이머를 돌리지 않습니다.
            if (!IsBossCleared())
                return;

            if (_fallbackMode == FallbackMode.ActivateClearObjects && AreRequiredObjectsVisible())
            {
                enabled = false;
                return;
            }

            if (!_timerStarted)
            {
                _timerStarted = true;
                _elapsedTime = 0f;
            }

            _elapsedTime += _useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;

            if (_elapsedTime < _fallbackDelay)
                return;

            ExecuteFallback();

            enabled = false;
        }

        private void ResolveReferences()
        {
            if (_fallbackMode == FallbackMode.ActivateClearObjects && !_stageManager)
                _stageManager = FindAnyObjectByType<SingleBossStageManager>(FindObjectsInactive.Include);

            if (_fallbackMode == FallbackMode.ChangeToEndingScene && !_finalBossStageManager)
                _finalBossStageManager = FindAnyObjectByType<FinalBossStageManager>(FindObjectsInactive.Include);

            if (!_gameServices)
                _gameServices = FindAnyObjectByType<GameServices>(FindObjectsInactive.Include);

            if (_stageManager)
            {
                // StageManager에 연결된 Box/Portal을 기본 대상으로 사용합니다.
                if (!_boxObject && _stageManager.Box)
                    _boxObject = _stageManager.Box.gameObject;

                if (!_portalObject && _stageManager.Portal)
                    _portalObject = _stageManager.Portal.gameObject;
            }

            if (!_clearObjectsRoot)
                _clearObjectsRoot = FindCommonRoot(_boxObject, _portalObject);
        }

        private bool IsBossCleared()
        {
            return _fallbackMode switch
            {
                FallbackMode.ActivateClearObjects =>
                    _stageManager && _stageManager.IsCleared,
                FallbackMode.ChangeToEndingScene =>
                    _finalBossStageManager
                    && _finalBossStageManager.CurrentPhase == FinalBossStageManager.Phase.StageCompleted,
                _ => false
            };
        }

        private bool AreRequiredObjectsVisible()
        {
            bool boxVisible = !_activateBox || (_boxObject && _boxObject.activeInHierarchy);
            bool portalVisible = !_activatePortal || (_portalObject && _portalObject.activeInHierarchy);
            return boxVisible && portalVisible;
        }

        private void ExecuteFallback()
        {
            switch (_fallbackMode)
            {
                case FallbackMode.ActivateClearObjects:
                    // 정상 시나리오가 놓친 경우에만 마지막으로 강제 활성화합니다.
                    ActivateClearObjects();
                    Debug.LogWarning(
                        $"[{nameof(BossClearObjectFallback)}] Boss was cleared, but clear objects were still hidden after {_fallbackDelay:0.##} seconds. Activated fallback objects.",
                        this);
                    break;

                case FallbackMode.ChangeToEndingScene:
                    ChangeToEndingScene();
                    break;
            }
        }

        private void ActivateClearObjects()
        {
            if (_clearObjectsRoot && !_clearObjectsRoot.activeSelf)
                _clearObjectsRoot.SetActive(true);

            if (_activateBox && _boxObject && !_boxObject.activeSelf)
                _boxObject.SetActive(true);

            if (_activatePortal && _portalObject && !_portalObject.activeSelf)
                _portalObject.SetActive(true);
        }

        private void ChangeToEndingScene()
        {
            if (!_gameServices)
            {
                Debug.LogError(
                    $"[{nameof(BossClearObjectFallback)}] {nameof(GameServices)} is missing, so fallback scene change cannot run.",
                    this);
                return;
            }

            // 엔딩 해금 상태를 남긴 뒤, 가능하면 기존 어두운 화면 전환을 재사용합니다.
            _gameServices.IsGameCleared = true;
            DarkscreenUI darkscreenUI = FindAnyObjectByType<DarkscreenUI>(FindObjectsInactive.Include);

            Debug.LogWarning(
                $"[{nameof(BossClearObjectFallback)}] Final boss was cleared, but ending flow did not finish after {_fallbackDelay:0.##} seconds. Changing to '{_endingSceneName}'.",
                this);

            if (darkscreenUI)
                darkscreenUI.CloseScreen(() => _gameServices.ChangeScene(_endingSceneName, this));
            else
                _gameServices.ChangeScene(_endingSceneName, this);
        }

        private static GameObject FindCommonRoot(GameObject first, GameObject second)
        {
            if (!first && !second)
                return null!;

            if (!first)
                return second.transform.parent ? second.transform.parent.gameObject : second;

            if (!second)
                return first.transform.parent ? first.transform.parent.gameObject : first;

            Transform candidate = first.transform;
            while (candidate)
            {
                if (second.transform.IsChildOf(candidate))
                    return candidate.gameObject;

                candidate = candidate.parent;
            }

            return first.transform.parent ? first.transform.parent.gameObject : first;
        }
    }
}
