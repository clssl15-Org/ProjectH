using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class TrainedSwordsmanshipManual : Relic
{
    public override void OnAcquire()
    {
        RelicManager.Instance.player.playerStats.attackPowerMultiplier += value * 0.01f ;
    }
    protected override void OnLoseCore()
    {
        Player player = RelicManager.Instance?.player;
        if (player != null)
        {
            player.playerStats.attackPowerMultiplier -= value * 0.01f;
        }
    }
}
