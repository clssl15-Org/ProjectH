using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using Infrastructure;

public class EndingVideoController : MonoBehaviour, IInjectable<GameServices>
{
    private const string RecordUnlockedSceneName = "Record_Unlocked";
    private const float TextMoveDuration = 25f;
    private const float VideoFadeOutDuration = 3f;
    private const float ThanksFadeInDuration = 1.5f;
    private const float ThanksDisplayDuration = 3f;
    private const float ThanksFadeOutDuration = 3f;

    [SerializeField]
    private VideoPlayer videoPlayer;

    [SerializeField]
    private float videoSpeed;

    [SerializeField]
    private Image videoFade;

    [SerializeField]
    private GameObject thanksPannel;

    [SerializeField]
    private Image thanksFade;

    [SerializeField]
    private TextMeshProUGUI txt;

    [SerializeField]
    private AudioSource endingAudioSource;

    private bool isEnd = false;
    private bool isThanksSequenceStarted = false;
    private bool isSceneTransitionRequested = false;

    private GameServices gameServices;

    public float ExpectedPlaybackDuration =>
        TextMoveDuration + VideoFadeOutDuration + ThanksFadeInDuration + ThanksDisplayDuration + ThanksFadeOutDuration;
    public bool HasRequestedSceneTransition => isSceneTransitionRequested;

    void IInjectable<GameServices>.Inject(GameServices gameServices)
        => this.gameServices = gameServices;

    private void Start()
    {
        StartCoroutine(Fade(videoFade, 1f, 0f, 0f));
        thanksPannel.SetActive(false);
        StartCoroutine(EndingSequence());
    }
    private void Update()
    {
        if (isEnd && (Input.GetKey(KeyCode.Mouse0) || Input.GetKey(KeyCode.Space)))
        {
            TryStartThanksSequence();
        }
    }
    IEnumerator EndingSequence()
    {
        StartCoroutine(Fade(videoFade, 1f, 0f, VideoFadeOutDuration));

        videoPlayer.Play();
        videoPlayer.playbackSpeed = videoSpeed;

        if (endingAudioSource != null)
            endingAudioSource.Play();

        yield return StartCoroutine(MoveY(txt.rectTransform, -550f, 822f, TextMoveDuration));

        isEnd = true;
        TryStartThanksSequence();
    }

    private void TryStartThanksSequence()
    {
        if (isThanksSequenceStarted)
            return;

        isEnd = false;
        isThanksSequenceStarted = true;
        StartCoroutine(GoThanks());
    }

    IEnumerator GoThanks()
    {
        if (endingAudioSource != null)
            endingAudioSource.Stop();

        yield return StartCoroutine(Fade(videoFade, 0f, 1f, VideoFadeOutDuration));

        thanksPannel.SetActive(true);
        thanksFade.gameObject.SetActive(true);

        yield return StartCoroutine(Fade(thanksFade, 1f, 0f, ThanksFadeInDuration));

        yield return new WaitForSeconds(ThanksDisplayDuration);

        yield return StartCoroutine(Fade(thanksFade, 0f, 1f, ThanksFadeOutDuration));

        RequestSceneTransition(this);
    }

    public bool RequestSceneTransition(object context = null, GameServices overrideGameServices = null)
    {
        if (isSceneTransitionRequested)
            return false;

        GameServices targetGameServices = overrideGameServices ? overrideGameServices : gameServices;
        if (!targetGameServices)
        {
            Debug.LogError(
                $"[{nameof(EndingVideoController)}] {nameof(GameServices)}가 없어 {RecordUnlockedSceneName} 씬으로 전환할 수 없습니다.",
                this);
            return false;
        }

        isSceneTransitionRequested = true;
        targetGameServices.ChangeScene(RecordUnlockedSceneName, context ?? this);
        return true;
    }

    IEnumerator Fade(Image fadePanel, float fromAlpha, float toAlpha, float fadeDuration)
    {
        float elapsedTime = 0f;
        Color panelColor = fadePanel.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / fadeDuration;

            float currentAlpha = Mathf.Lerp(fromAlpha, toAlpha, t);
            panelColor.a = currentAlpha;
            fadePanel.color = panelColor;

            yield return null;
        }

        panelColor.a = toAlpha;
        fadePanel.color = panelColor;
    }

    IEnumerator MoveY(RectTransform target, float startY, float endY, float duration)
    {
        float elapsedTime = 0f;
        float fixedX = target.anchoredPosition.x;

        while (elapsedTime < duration)
        {
            float speedMultiplier = Input.GetMouseButton(0) || Input.GetKey(KeyCode.Space) ? 5f : 1f;

            elapsedTime += Time.deltaTime * speedMultiplier;

            float t = Mathf.Clamp01(elapsedTime / duration);

            float currentY = Mathf.Lerp(startY, endY, t);
            target.anchoredPosition = new Vector2(fixedX, currentY);

            yield return null;
        }

        target.anchoredPosition = new Vector2(fixedX, endY);
    }
}
