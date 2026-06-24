using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actors.PlayerSystem;

public class DevotedKnightFragmentShield : Relic
{
    public override void OnAcquire()
    {
        RelicManager.Instance.player.sieldCount += (int)value;
        RelicManager.Instance.player.PlayerHealth.OnSieldBreak += OnLose;
    }

    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.PlayerHealth.OnSieldBreak -= OnLose;
    }
}
