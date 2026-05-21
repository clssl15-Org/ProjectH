using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class GaleKnightBoots : Relic
{
    private CooldownTimer cooldownTimer;
    public override void OnAcquire()
    {
        RelicManager.Instance.player.playerStats.moveSpeedMultiplier += value;


        cooldownTimer = gameObject.AddComponent<CooldownTimer>();
        cooldownTimer.StartCooldown(value * 60, Time.fixedDeltaTime);
    }
    private void Update()
    {
        if (!cooldownTimer || !cooldownTimer.IsOnCooldown)
        {
            base.OnLose();
        }
    }

    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.playerStats.moveSpeedMultiplier -= value;
    }
}