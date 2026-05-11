using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SacredProtectionMark : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.maxHeathMultiplier += value * 0.01f;
        RelicManager.Instance.player.PlayerHealth.ChangeMaxHealth();
    }
    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.playerStats.maxHeathMultiplier -= value * 0.01f;
        RelicManager.Instance.player.PlayerHealth.ChangeMaxHealth();
    }
}
