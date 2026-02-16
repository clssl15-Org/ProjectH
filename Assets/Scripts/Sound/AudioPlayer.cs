using System;
using Infrastructure;
using UnityEngine;

namespace Sound
{
    public abstract class AudioPlayer<TAudioName, TAudioPlayerManager> : MonoBehaviour,
        IInjectable<TAudioPlayerManager>
        where TAudioName : Enum
        where TAudioPlayerManager : AudioPlayManager<TAudioName>
    {
        protected TAudioPlayerManager AudioPlayManager { get; private set; }

        void IInjectable<TAudioPlayerManager>.Inject(TAudioPlayerManager playManager) =>
            AudioPlayManager = playManager;
    }
}
