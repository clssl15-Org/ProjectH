using System;
using Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Sound
{
    public abstract class AudioPlayer<TName, TAudioPlayerManager> : MonoBehaviour,
        IPointerEnterHandler,
        IPointerClickHandler,
        IInjectable<TAudioPlayerManager>
        where TName : Enum
        where TAudioPlayerManager : AudioPlayManager<TName>
    {
        protected TAudioPlayerManager AudioPlayManager { get; private set; }

        void IInjectable<TAudioPlayerManager>.Inject(TAudioPlayerManager playManager) =>
            AudioPlayManager = playManager;

        public virtual void OnPointerEnter(PointerEventData _) { }
        public virtual void OnPointerClick(PointerEventData _) { }
    }
}
