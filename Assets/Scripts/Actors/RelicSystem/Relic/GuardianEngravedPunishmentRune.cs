using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianEngravedPunishmentRune : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.skillPowerMultiplier += value * 0.01f;
    }
    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.playerStats.skillPowerMultiplier -= value * 0.01f;
    }
}
