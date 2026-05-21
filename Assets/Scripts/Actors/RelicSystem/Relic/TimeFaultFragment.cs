using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFaultFragment : Relic
{
    public override void OnAcquire()
    {
        RelicManager.Instance.player.playerStats.skillCooldownMultiplier += value * 0.01f;
    }
    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.playerStats.skillCooldownMultiplier -= value * 0.01f;
    }
}