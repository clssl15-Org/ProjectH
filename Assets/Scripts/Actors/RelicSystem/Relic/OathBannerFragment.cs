using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class OathBannerFragment : Relic, IRelicPlayerRebindHandler
{
    private int _appliedPlayerInstanceId;

    public override void OnAcquire()
    {
        ApplyToPlayer(RelicManager.Instance.player, addSkillIfMissing: true);
    }

    public void RebindPlayer(Player player)
    {
        ApplyToPlayer(player, addSkillIfMissing: true);
    }

    protected override void OnLoseCore()
    {
        var player = RelicManager.Instance != null ? RelicManager.Instance.player : null;
        if (!player || _appliedPlayerInstanceId != player.GetInstanceID())
        {
            _appliedPlayerInstanceId = 0;
            return;
        }

        var skill = GetSkill(player);
        skill.BonusMultiplier -= value * 0.01f;
        _appliedPlayerInstanceId = 0;
    }

    private void ApplyToPlayer(Player player, bool addSkillIfMissing)
    {
        var skill = GetSkill(player);

        if (_appliedPlayerInstanceId != player.GetInstanceID())
        {
            skill.BonusMultiplier += value * 0.01f;
            _appliedPlayerInstanceId = player.GetInstanceID();
        }

        if (addSkillIfMissing && !player.SkillManager.skills.Contains(skill))
            player.SkillManager.AddSkill(skill);
    }

    private static Skill3 GetSkill(Player player)
    {
        if (player == null)
            throw new System.ArgumentNullException(nameof(player));

        if (player.StatesGO == null)
            throw new System.InvalidOperationException("[OathBannerFragment] 플레이어의 StatesGO가 할당되지 않았습니다.");

        var skill = player.StatesGO.GetComponent<Skill3>();
        if (skill == null)
            throw new System.InvalidOperationException("[OathBannerFragment] Skill3 컴포넌트를 찾을 수 없습니다.");

        return skill;
    }
}
