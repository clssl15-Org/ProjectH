using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiantHeartstone : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.ultimateCooldownMultiplier += value * 0.01f;
    }
    public override void OnLose()
    {
        RelicManager.Instance.player.playerStats.ultimateCooldownMultiplier -= value * 0.01f;
    }
}