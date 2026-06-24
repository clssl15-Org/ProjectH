using Actors.PlayerSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KnighthoodLastOathStone : Relic
{
    private static KnighthoodLastOathStone? primaryInstance;

    private PlayerHealth? playerHealth;
    private bool isBuffActive;

    public override void OnAcquire()
    {
        var manager = RelicManager.Instance;
        int relicId = data.RelicNumber;
        int stackCount = manager.OwnedRelics.TryGetValue(relicId, out var owned)
            ? owned.Count
            : 1;

        if (stackCount <= 1)
        {
            BecomePrimary();
            return;
        }

        primaryInstance?.EvaluateBuffState();
        enabled = false;
    }

    protected override void OnLoseCore()
    {
        if (this == primaryInstance)
        {
            TearDownPrimary();

            if (TryPromoteNextPrimary())
                return;
        }
        else
        {
            primaryInstance?.EvaluateBuffState(excludeFromSum: this);
        }
    }

    private void BecomePrimary(KnighthoodLastOathStone? excludeFromSum = null)
    {
        primaryInstance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SubscribeToPlayerHealth();
        EvaluateBuffState(excludeFromSum);
    }

    private void TearDownPrimary()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UnsubscribeFromPlayerHealth();

        if (isBuffActive)
        {
            isBuffActive = false;
            ApplyAttackPowerMultiplier(0.5f);
            ApplyRangedBonusMultiplier(0.5f);
        }

        primaryInstance = null;
    }

    private bool TryPromoteNextPrimary()
    {
        if (!RelicManager.Instance.OwnedRelics.TryGetValue(data.RelicNumber, out var list))
            return false;

        foreach (var relicObj in list)
        {
            if (relicObj == null || relicObj == gameObject)
                continue;

            if (!relicObj.TryGetComponent<KnighthoodLastOathStone>(out var next))
                continue;

            next.enabled = true;
            next.BecomePrimary(excludeFromSum: this);
            return true;
        }

        return false;
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
        playerHealth.Damaged += OnHealthChanged;
        playerHealth.Healed += OnHealthChanged;
    }

    private void UnsubscribeFromPlayerHealth()
    {
        if (playerHealth == null)
            return;

        playerHealth.Damaged -= OnHealthChanged;
        playerHealth.Healed -= OnHealthChanged;
        playerHealth = null;
    }

    private void OnHealthChanged() => EvaluateBuffState();

    private void EvaluateBuffState(KnighthoodLastOathStone? excludeFromSum = null)
    {
        if (this != primaryInstance || playerHealth == null)
            return;

        float thresholdPercent = GetCombinedThresholdPercent(excludeFromSum);
        float healthThreshold = playerHealth.MaxHealth * thresholdPercent * 0.01f;
        bool shouldBeActive = playerHealth.CurrentHealth <= healthThreshold;

        if (shouldBeActive && !isBuffActive)
        {
            isBuffActive = true;
            ApplyAttackPowerMultiplier(2f);
            ApplyRangedBonusMultiplier(2f);
        }
        else if (!shouldBeActive && isBuffActive)
        {
            isBuffActive = false;
            ApplyAttackPowerMultiplier(0.5f);
            ApplyRangedBonusMultiplier(0.5f);
        }
    }

    private static void ApplyAttackPowerMultiplier(float factor)
    {
        var player = RelicManager.Instance.player;
        var stats = player.playerStats;
        stats.attackPowerMultiplier *= factor;
        player.playerStats = stats;
    }

    private static void ApplyRangedBonusMultiplier(float factor)
    {
        var player = RelicManager.Instance.player;
        if (player?.StatesGO == null)
            return;

        if (!player.StatesGO.TryGetComponent<RangedAttack>(out var rangedAttack))
            return;

        rangedAttack.BonusMultiplier *= factor;
    }

    private float GetCombinedThresholdPercent(KnighthoodLastOathStone? excludeFromSum = null)
    {
        float sum = RelicManager.Instance.GetValueSum(data.RelicNumber);

        if (excludeFromSum != null)
            sum -= excludeFromSum.value;

        return sum;
    }

    private void OnDestroy()
    {
        if (this == primaryInstance)
            primaryInstance = null;
    }
}
