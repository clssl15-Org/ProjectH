using Infrastructure;
using UnityEngine;

public class EndingSceneChangeFallback : MonoBehaviour, IInjectable<GameServices>
{
    private const float DefaultPlaybackDuration = 35.5f;

    [SerializeField] private EndingVideoController endingVideoController;
    [SerializeField, Min(0f)] private float extraDelay = 2.5f;

    private GameServices gameServices;
    private float elapsedTime;
    private bool fallbackRequested;

    void IInjectable<GameServices>.Inject(GameServices gameServices)
        => this.gameServices = gameServices;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Update()
    {
        if (fallbackRequested)
            return;

        ResolveReferences();

        if (endingVideoController && endingVideoController.HasRequestedSceneTransition)
        {
            enabled = false;
            return;
        }

        elapsedTime += Time.unscaledDeltaTime;

        if (elapsedTime < GetFallbackDelay())
            return;

        RequestFallbackSceneTransition();
    }

    private void ResolveReferences()
    {
        if (!endingVideoController)
            endingVideoController = FindAnyObjectByType<EndingVideoController>(FindObjectsInactive.Include);

        if (!gameServices)
            gameServices = FindAnyObjectByType<GameServices>(FindObjectsInactive.Include);
    }

    private float GetFallbackDelay()
    {
        float playbackDuration = endingVideoController
            ? endingVideoController.ExpectedPlaybackDuration
            : DefaultPlaybackDuration;

        return playbackDuration + extraDelay;
    }

    private void RequestFallbackSceneTransition()
    {
        fallbackRequested = true;

        if (endingVideoController && endingVideoController.RequestSceneTransition(this, gameServices))
        {
            enabled = false;
            return;
        }

        Debug.LogError(
            $"[{nameof(EndingSceneChangeFallback)}] 엔딩 씬 폴백 전환을 실행하지 못했습니다.",
            this);
    }
}
