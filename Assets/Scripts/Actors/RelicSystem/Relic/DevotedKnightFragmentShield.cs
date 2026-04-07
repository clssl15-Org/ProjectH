using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevotedKnightFragmentShield : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.sieldCount += (int)value;
        RelicManager.Instance.player.PlayerHealth.OnSieldBreak += OnLose;
    }

    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.PlayerHealth.OnSieldBreak -= OnLose;
    }
}
