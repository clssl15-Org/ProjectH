using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainedSwordsmanshipManual : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.attackPowerMultiplier += value;
    }
    public override void OnLose()
    {
        RelicManager.Instance.player.playerStats.attackPowerMultiplier -= value;
    }
}
