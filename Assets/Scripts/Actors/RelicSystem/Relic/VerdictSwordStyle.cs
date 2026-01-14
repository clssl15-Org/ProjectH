using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class VerdictSwordStyle : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        Player player = RelicManager.Instance.player;
        StrongAttack firstSkill = player.StatesGO.GetComponent<StrongAttack>();
        firstSkill.BonusMultiplier += value;
        player.SkillManager.AddSkill(firstSkill);
    }
    public override void OnLose()
    {

    }
}