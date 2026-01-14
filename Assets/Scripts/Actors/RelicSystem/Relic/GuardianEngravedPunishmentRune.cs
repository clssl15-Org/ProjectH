using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianEngravedPunishmentRune : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.skillPowerMultiplier += value;
    }
    public override void OnLose()
    {
        RelicManager.Instance.player.playerStats.skillPowerMultiplier -= value;
    }
}
