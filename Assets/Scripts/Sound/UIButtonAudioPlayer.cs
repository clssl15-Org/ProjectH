using BlackboxSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sound
{
    public class UIButtonAudioPlayer : AudioPlayer<SfxName, SfxPlayManager>,
        IPointerEnterHandler,
        IPointerClickHandler
    {
        public void OnPointerEnter(PointerEventData _)
        {
            if (AudioPlayManager)
                AudioPlayManager.Play(SfxName.Hover);
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "AudioPlayManager가 유효하지 않기 떄문에 Hover 소리를 재생할 수 없습니다."),
                    this);
        }

        public void OnPointerClick(PointerEventData _)
        {
            if (AudioPlayManager)
                AudioPlayManager.Play(SfxName.Click);
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "AudioPlayManager가 유효하지 않기 떄문에 Click 소리를 재생할 수 없습니다."),
                    this);
        }
    }
}
