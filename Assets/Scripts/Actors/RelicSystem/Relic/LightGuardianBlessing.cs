using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rules;
using Actors.PlayerSystem;

public class LightGuardianBlessing : Relic
{
    private PlayerHealth playerHealth;
    public override void OnAcquire()
    {
        playerHealth = RelicManager.Instance.player.GetComponent<PlayerHealth>();
        GameEvents.OnMonsterDied += OnPlayerHeal;
    }
    protected override void OnLoseCore()
    {
        GameEvents.OnMonsterDied -= OnPlayerHeal;
    }

    public void OnPlayerHeal()
    {
        playerHealth.HealByPercent(value * 0.01f);
    }
}
