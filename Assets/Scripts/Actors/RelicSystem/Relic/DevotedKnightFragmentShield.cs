using Actors.PlayerSystem;
using UnityEngine;

public class DevotedKnightFragmentShield : Relic, IRelicPlayerRebindHandler
{
    private PlayerHealth subscribedPlayerHealth;

    public override void OnAcquire()
    {
        var player = GetPlayer();
        player.sieldCount += Mathf.RoundToInt(value);
        SubscribeToShieldBreak(player);
    }

    public void RebindPlayer(Player player)
    {
        SubscribeToShieldBreak(player);
    }

    protected override void OnLoseCore()
    {
        UnsubscribeFromShieldBreak();
    }

    private void SubscribeToShieldBreak(Player player)
    {
        if (player == null)
            throw new System.ArgumentNullException(nameof(player));

        var playerHealth = player.PlayerHealth;
        if (playerHealth == null)
            throw new System.InvalidOperationException("[DevotedKnightFragmentShield] 플레이어의 PlayerHealth를 찾을 수 없습니다.");

        if (subscribedPlayerHealth == playerHealth)
            return;

        UnsubscribeFromShieldBreak();
        subscribedPlayerHealth = playerHealth;
        subscribedPlayerHealth.OnSieldBreak += OnLose;
    }

    private void UnsubscribeFromShieldBreak()
    {
        if (subscribedPlayerHealth != null)
            subscribedPlayerHealth.OnSieldBreak -= OnLose;

        subscribedPlayerHealth = null;
    }

    private static Player GetPlayer()
    {
        var player = RelicManager.Instance?.player ?? FindObjectOfType<Player>();
        if (player == null)
            throw new System.InvalidOperationException("[DevotedKnightFragmentShield] 플레이어를 찾을 수 없습니다.");

        return player;
    }

    private void OnDestroy()
    {
        UnsubscribeFromShieldBreak();
    }
}
