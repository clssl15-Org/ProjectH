using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class ScoutAccuracyGlasses : Relic
{
    public override void OnAcquire()
    {
        Player player = RelicManager.Instance.player;
        RangedAttack skill = player.StatesGO.GetComponent<RangedAttack>();
        skill.BonusMultiplier += value * 0.01f;
    }
    protected override void OnLoseCore()
    {
        Player player = RelicManager.Instance?.player;
        if (player != null && player.StatesGO != null)
        {
            RangedAttack skill = player.StatesGO.GetComponent<RangedAttack>();
            if (skill != null)
            {
                skill.BonusMultiplier -= value * 0.01f;
            }
        }
    }
}