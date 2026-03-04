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

        private Vector3 _enableMousePosition;
        private bool _canTrigger = false;
        private const float MoveThreshold = 1f;


        private void OnEnable()
        {
            _enableMousePosition = Input.mousePosition;
            _canTrigger = false;
        }

        private void Update()
        {
            if (!_canTrigger)
            {
                if (Vector3.Distance(_enableMousePosition, Input.mousePosition) > MoveThreshold)
                    _canTrigger = true;
            }
        }

        public void OnPointerEnter(PointerEventData _)
        {
            if (!_canTrigger)
                return;

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

        public void OnPointerExit(PointerEventData _)
        {
            _canTrigger = true;
        }
    }
}
