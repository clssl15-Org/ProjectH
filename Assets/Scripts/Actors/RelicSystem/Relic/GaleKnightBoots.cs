using Actors.PlayerSystem;
using UnityEngine;

public class GaleKnightBoots : Relic
{
    [SerializeField] private float moveSpeedBonus = 0.2f;

    private static GaleKnightBoots primaryInstance;

    private CooldownTimer cooldownTimer;

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

        ExtendPrimaryDuration();
        enabled = false;
    }

    private void BecomePrimary()
    {
        primaryInstance = this;
        RelicManager.Instance.player.playerStats.moveSpeedMultiplier += moveSpeedBonus;

        cooldownTimer = gameObject.AddComponent<CooldownTimer>();
        cooldownTimer.StartCooldown(value * 60f, Time.fixedDeltaTime);
    }

    private void ExtendPrimaryDuration()
    {
        if (primaryInstance == null)
            return;

        if (primaryInstance.cooldownTimer == null)
            primaryInstance.cooldownTimer = primaryInstance.GetComponent<CooldownTimer>();

        if (primaryInstance.cooldownTimer != null)
            primaryInstance.cooldownTimer.AddDuration(value * 60f);
    }

    private void Update()
    {
        if (this != primaryInstance)
            return;

        if (!cooldownTimer || !cooldownTimer.IsOnCooldown)
            ExpireBuff();
    }

    private void ExpireBuff()
    {
        OnLoseCore();

        int relicId = data.RelicNumber;
        var manager = RelicManager.Instance;
        while (manager.OwnedRelics.TryGetValue(relicId, out var list) && list.Count > 0)
            manager.RemoveRelic(relicId);

        ClearStaticState();
    }

    protected override void OnLoseCore()
    {
        if (this != primaryInstance)
            return;

        RelicManager.Instance.player.playerStats.moveSpeedMultiplier -= moveSpeedBonus;
    }

    private void OnDestroy()
    {
        if (this == primaryInstance)
            primaryInstance = null;
    }

    private static void ClearStaticState() => primaryInstance = null;
}
