using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiantHeartstone : Relic
{
    public override void OnAcquire()
    {
        RelicManager.Instance.player.playerStats.ultimateCooldownMultiplier += value * 0.01f;
    }
    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.playerStats.ultimateCooldownMultiplier -= value * 0.01f;
    }
}