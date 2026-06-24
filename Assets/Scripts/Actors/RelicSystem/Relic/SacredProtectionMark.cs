using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SacredProtectionMark : Relic
{
    public override void OnAcquire()
    {
        var player = RelicManager.Instance.player;
        var health = player.PlayerHealth;
        int previousMax = health.MaxHealth;

        var stats = player.playerStats;
        stats.maxHeathMultiplier += value * 0.01f;
        player.playerStats = stats;

        health.NotifyMaxHealthChanged(previousMax);
    }

    protected override void OnLoseCore()
    {
        var player = RelicManager.Instance?.player;
        if (player != null)
        {
            var health = player.PlayerHealth;
            if (health != null)
            {
                int previousMax = health.MaxHealth;
                var stats = player.playerStats;
                stats.maxHeathMultiplier -= value * 0.01f;
                player.playerStats = stats;
                health.NotifyMaxHealthChanged(previousMax);
            }
        }
    }
}
