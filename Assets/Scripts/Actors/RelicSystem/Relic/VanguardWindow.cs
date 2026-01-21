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
        firstSkill.BonusMultiplier += value;
        player.SkillManager.AddSkill(firstSkill);
    }
    public override void OnLose()
    {

    }
}
