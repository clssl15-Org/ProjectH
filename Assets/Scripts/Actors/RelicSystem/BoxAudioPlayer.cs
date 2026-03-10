using Sound;
using UnityEngine;

[RequireComponent(typeof(Box))]
public class BoxAudioPlayer : SfxAudioController
{
    protected override void Awake()
    {
        base.Awake();
        GetComponent<Box>().Opening += () => Play("Open");
    }
}
