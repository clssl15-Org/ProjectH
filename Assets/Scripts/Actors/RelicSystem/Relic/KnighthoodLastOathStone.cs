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
        OnReinforcedAcquire();

        playerHealth = RelicManager.Instance.player.PlayerHealth;
        playerHealth.Damaged += Activate;
        playerHealth.Healed += Activate;
    }
    public override void OnLose()
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
            if (playerHealth.CurrentHealth <= playerHealth.MaxHealth * value)
            {
                isActivated = true;
                RelicManager.Instance.player.playerStats.attackPowerMultiplier *= 2;
            }
        }
        else if (isActivated)
        {
            if (playerHealth.CurrentHealth > playerHealth.MaxHealth * value)
            {
                isActivated = false;
                RelicManager.Instance.player.playerStats.attackPowerMultiplier /= 2;
            }
        }
    }
}
