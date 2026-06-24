using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFaultFragment : Relic
{
    public override void OnAcquire()
    {
        ref var stats = ref RelicManager.Instance.player.playerStats;
        stats.skillCooldownMultiplier = Mathf.Max(0.01f, stats.skillCooldownMultiplier - value * 0.01f);
    }

    protected override void OnLoseCore()
    {
        ref var stats = ref RelicManager.Instance.player.playerStats;
        stats.skillCooldownMultiplier += value * 0.01f;
    }
}