using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class VerdictSwordStyle : Relic
{
    public override void OnAcquire()
    {
        Player player = RelicManager.Instance.player;
        StrongAttack skill = player.StatesGO.GetComponent<StrongAttack>();
        skill.BonusMultiplier += value * 0.01f;
        player.SkillManager.AddSkill(skill);
    }
    protected override void OnLoseCore()
    {

    }
}