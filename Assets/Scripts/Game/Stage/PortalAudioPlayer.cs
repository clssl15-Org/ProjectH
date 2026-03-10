using Sound;
using UnityEngine;

[RequireComponent(typeof(Portal))]
public class PortalAudioPlayer : SfxAudioController
{
    protected override void Awake()
    {
        base.Awake();
        GetComponent<Portal>().Opening += () => Play("Open");
        GetComponent<Portal>().Closing += () => Play("Move");
    }
}
