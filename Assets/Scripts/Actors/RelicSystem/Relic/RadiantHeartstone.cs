using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RadiantHeartstone : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.ultimateCooldownMultiplier += value;
    }
    public override void OnLose()
    {
        RelicManager.Instance.player.playerStats.ultimateCooldownMultiplier -= value;
    }
}