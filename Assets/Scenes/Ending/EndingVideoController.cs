using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Infrastructure;

public class EndingVideoController : MonoBehaviour, IInjectable<GameServices>
{
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

    private GameServices gameServices;

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
            isEnd = false;
            StartCoroutine(GoThanks());
        }
    }
    IEnumerator EndingSequence()
    {
        StartCoroutine(Fade(videoFade, 1f, 0f, 3f));

        videoPlayer.Play();
        videoPlayer.playbackSpeed = videoSpeed;

        if (endingAudioSource != null)
            endingAudioSource.Play();

        yield return StartCoroutine(MoveY(txt.rectTransform, -400f, 822f, 25f));

        isEnd = true;
    }
    IEnumerator GoThanks()
    {
        if (endingAudioSource != null)
            endingAudioSource.Stop();

        yield return StartCoroutine(Fade(videoFade, 0f, 1f, 3f));

        thanksPannel.SetActive(true);
        thanksFade.gameObject.SetActive(true);

        yield return StartCoroutine(Fade(thanksFade, 1f, 0f, 1.5f));

        yield return new WaitForSeconds(3f);

        yield return StartCoroutine(Fade(thanksFade, 0f, 1f, 3f));

        //SceneManager.LoadScene("Title");
        gameServices.ChangeScene("Record_Unlocked", this);
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
