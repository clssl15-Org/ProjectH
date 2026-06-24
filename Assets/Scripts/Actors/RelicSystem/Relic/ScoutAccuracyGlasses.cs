using Actors.PlayerSystem;
using UnityEngine;

public class ScoutAccuracyGlasses : Relic, IRelicPlayerRebindHandler
{
    private static ScoutAccuracyGlasses? primaryInstance;

    private int _appliedPlayerInstanceId;
    private float _appliedBonus;

    public override void OnAcquire()
    {
        if (primaryInstance == null)
        {
            BecomePrimary();
            return;
        }

        primaryInstance.SyncBonus();
    }

    public void RebindPlayer(Player player)
    {
        if (this != primaryInstance)
            return;

        SyncBonus(player);
    }

    protected override void OnLoseCore()
    {
        if (this == primaryInstance)
        {
            ClearAppliedBonus();

            if (TryPromoteNextPrimary())
                return;

            primaryInstance = null;
            return;
        }

        primaryInstance?.SyncBonus(excludeFromSum: this);
    }

    private void BecomePrimary(ScoutAccuracyGlasses? excludeFromSum = null)
    {
        primaryInstance = this;
        SyncBonus(excludeFromSum: excludeFromSum);
    }

    private bool TryPromoteNextPrimary()
    {
        if (RelicManager.Instance == null)
            return false;

        if (!RelicManager.Instance.OwnedRelics.TryGetValue(data.RelicNumber, out var list))
            return false;

        foreach (var relicObj in list)
        {
            if (relicObj == null || relicObj == gameObject)
                continue;

            if (!relicObj.TryGetComponent<ScoutAccuracyGlasses>(out var next))
                continue;

            next.BecomePrimary(excludeFromSum: this);
            return true;
        }

        return false;
    }

    private void SyncBonus(Player? targetPlayer = null, ScoutAccuracyGlasses? excludeFromSum = null)
    {
        var player = targetPlayer != null
            ? targetPlayer
            : RelicManager.Instance?.player ?? FindObjectOfType<Player>();

        var skill = GetSkill(player);
        int playerInstanceId = player.GetInstanceID();

        if (_appliedPlayerInstanceId == playerInstanceId)
            skill.BonusMultiplier -= _appliedBonus;

        _appliedBonus = GetCombinedBonus(excludeFromSum);
        skill.BonusMultiplier += _appliedBonus;
        _appliedPlayerInstanceId = playerInstanceId;
    }

    private void ClearAppliedBonus()
    {
        var player = RelicManager.Instance != null ? RelicManager.Instance.player : null;
        if (!player || _appliedPlayerInstanceId != player.GetInstanceID())
        {
            _appliedBonus = 0f;
            _appliedPlayerInstanceId = 0;
            return;
        }

        var skill = GetSkill(player);
        skill.BonusMultiplier -= _appliedBonus;
        _appliedBonus = 0f;
        _appliedPlayerInstanceId = 0;
    }

    private float GetCombinedBonus(ScoutAccuracyGlasses? excludeFromSum = null)
    {
        if (RelicManager.Instance == null)
            return 0f;

        float sum = RelicManager.Instance.GetValueSum(data.RelicNumber);

        if (excludeFromSum != null)
            sum -= excludeFromSum.value;

        return sum * 0.01f;
    }

    private static RangedAttack GetSkill(Player player)
    {
        if (player == null)
            throw new System.ArgumentNullException(nameof(player));

        if (player.StatesGO == null)
            throw new System.InvalidOperationException("[ScoutAccuracyGlasses] 플레이어의 StatesGO가 할당되지 않았습니다.");

        var skill = player.StatesGO.GetComponent<RangedAttack>();
        if (skill == null)
            throw new System.InvalidOperationException("[ScoutAccuracyGlasses] RangedAttack 컴포넌트를 찾을 수 없습니다.");

        return skill;
    }

    private void OnDestroy()
    {
        if (this == primaryInstance)
        {
            ClearAppliedBonus();

            if (!TryPromoteNextPrimary())
                primaryInstance = null;

            return;
        }

        primaryInstance?.SyncBonus();
    }
}
