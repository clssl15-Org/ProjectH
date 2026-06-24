using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class OathBannerFragment : Relic
{
    public override void OnAcquire()
    {
        Player player = RelicManager.Instance.player;
        Skill3 firstSkill = player.StatesGO.GetComponent<Skill3>();
        firstSkill.BonusMultiplier += value * 0.01f;
        player.SkillManager.AddSkill(firstSkill);
    }
    protected override void OnLoseCore()
    {
        Player player = RelicManager.Instance?.player;
        if (player != null && player.StatesGO != null)
        {
            Skill3 firstSkill = player.StatesGO.GetComponent<Skill3>();
            if (firstSkill != null)
            {
                firstSkill.BonusMultiplier -= value * 0.01f;
            }
        }
    }
}