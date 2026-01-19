using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class GaleKnightBoots : Relic
{
    private CooldownTiemr cooldownTimer;
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.moveSpeedMultiplier += value;


        cooldownTimer = gameObject.AddComponent<CooldownTiemr>();
        cooldownTimer.StartCooldown(value * 60, Time.deltaTime);
    }
    private void Update()
    {
        if (!cooldownTimer || !cooldownTimer.IsOnCooldown)
        {
            RelicManager.Instance.RemoveRelic(data.RelicNumber);
        }
    }

    public override void OnLose()
    {
        RelicManager.Instance.player.playerStats.moveSpeedMultiplier -= value;
    }
}