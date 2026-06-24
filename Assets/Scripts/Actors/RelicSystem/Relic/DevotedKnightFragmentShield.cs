using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevotedKnightFragmentShield : Relic
{
    public override void OnAcquire()
    {
        RelicManager.Instance.player.sieldCount += (int)value;
        RelicManager.Instance.player.PlayerHealth.OnSieldBreak += OnLose;
    }

    protected override void OnLoseCore()
    {
        Player player = RelicManager.Instance?.player;
        if (player != null && player.PlayerHealth != null)
        {
            player.PlayerHealth.OnSieldBreak -= OnLose;
        }
    }
}
