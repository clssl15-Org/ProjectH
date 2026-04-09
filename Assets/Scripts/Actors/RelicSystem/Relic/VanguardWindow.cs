using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class VanguardWindow : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        Player player = RelicManager.Instance.player;
        RushStabbing firstSkill = player.StatesGO.GetComponent<RushStabbing>();
        firstSkill.BonusMultiplier += value * 0.01f;
        player.SkillManager.AddSkill(firstSkill);
    }
    protected override void OnLoseCore()
    {

    }
}
