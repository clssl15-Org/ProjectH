using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sound
{
    public class UIAudioPlayer : AudioPlayer<SfxType, SfxPlayManager>
    {
        public override void OnPointerEnter(PointerEventData _)
        {
            AudioPlayManager.Play(SfxType.Hover);
        }

        public override void OnPointerClick(PointerEventData _)
        {
            AudioPlayManager.Play(SfxType.Click);
        }
    }
}
