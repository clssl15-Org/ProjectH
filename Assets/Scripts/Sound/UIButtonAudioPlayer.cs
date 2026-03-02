using BlackboxSystem;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sound
{
    public class UIButtonAudioPlayer : AudioPlayer<SfxName, SfxPlayManager>,
        IPointerEnterHandler,
        IPointerClickHandler
    {
        [field: SerializeField] public SfxName HoverSound { get; set; } = SfxName.Hover;
        [field: SerializeField] public SfxName ClickSound { get; set; } = SfxName.Click;

        public void OnPointerEnter(PointerEventData _)
        {
            if (AudioPlayManager)
                AudioPlayManager.Play(HoverSound);
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "AudioPlayManager가 유효하지 않기 떄문에 Hover 소리를 재생할 수 없습니다."),
                    this);
        }

        public void OnPointerClick(PointerEventData _)
        {
            if (AudioPlayManager)
                AudioPlayManager.Play(ClickSound);
            else
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "AudioPlayManager가 유효하지 않기 떄문에 Click 소리를 재생할 수 없습니다."),
                    this);
        }
    }
}
