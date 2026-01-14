using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeFaultFragment : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.skillCooldownMultiplier += value;
    }
    public override void OnLose()
    {
        RelicManager.Instance.player.playerStats.skillCooldownMultiplier -= value;
    }
}