using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;
using UnityEngine.Networking;

public class KnighthoodLastOathStone : Relic
{
    private PlayerHealth playerHealth;
    private bool isActivated = false;
    public override void OnAcquire()
    {
        playerHealth = RelicManager.Instance.player.PlayerHealth;
        playerHealth.Damaged += Activate;
        playerHealth.Healed += Activate;
    }
    protected override void OnLoseCore()
    {
        playerHealth.Damaged -= Activate;
        playerHealth.Healed -= Activate;

        if (isActivated)
        {
            isActivated = false;
            RelicManager.Instance.player.playerStats.attackPowerMultiplier /= 2;
        }
    }
    public void Activate()
    {
        if (!isActivated)
        {
            if (playerHealth.CurrentHealth <= playerHealth.MaxHealth * value * 0.01f)
            {
                isActivated = true;
                RelicManager.Instance.player.playerStats.attackPowerMultiplier *= 2;
            }
        }
        else if (isActivated)
        {
            if (playerHealth.CurrentHealth > playerHealth.MaxHealth * value * 0.01f)
            {
                isActivated = false;
                RelicManager.Instance.player.playerStats.attackPowerMultiplier /= 2;
            }
        }
    }
}
