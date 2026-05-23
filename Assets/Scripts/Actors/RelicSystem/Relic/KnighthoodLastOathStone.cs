using Actors.PlayerSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KnighthoodLastOathStone : Relic
{
    private PlayerHealth? playerHealth;
    private bool isActivated;

    public override void OnAcquire()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        SubscribeToPlayerHealth();
        Activate();
    }

    protected override void OnLoseCore()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayerHealth();

        if (!isActivated)
            return;

        isActivated = false;
        RelicManager.Instance.player.playerStats.attackPowerMultiplier /= 2;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => SubscribeToPlayerHealth();

    private void SubscribeToPlayerHealth()
    {
        UnsubscribeFromPlayerHealth();

        var player = RelicManager.Instance?.player ?? FindObjectOfType<Player>();
        if (player == null)
        {
            Debug.LogWarning("[KnighthoodLastOathStone] Player not found; Damaged/Healed not subscribed.");
            return;
        }

        playerHealth = player.PlayerHealth;
        playerHealth.Damaged += Activate;
        playerHealth.Healed += Activate;
    }

    private void UnsubscribeFromPlayerHealth()
    {
        if (playerHealth == null)
            return;

        playerHealth.Damaged -= Activate;
        playerHealth.Healed -= Activate;
        playerHealth = null;
    }

    public void Activate()
    {
        if (playerHealth == null)
            return;

        float healthThreshold = playerHealth.MaxHealth * value * 0.01f;

        if (!isActivated)
        {
            if (playerHealth.CurrentHealth <= healthThreshold)
            {
                isActivated = true;
                RelicManager.Instance.player.playerStats.attackPowerMultiplier *= 2;
            }
        }
        else if (playerHealth.CurrentHealth > healthThreshold)
        {
            isActivated = false;
            RelicManager.Instance.player.playerStats.attackPowerMultiplier /= 2;
        }
    }
}
