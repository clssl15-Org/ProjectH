using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class ScoutAccuracyGlasses : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        Player player = RelicManager.Instance.player;
        RangedAttack skill = player.StatesGO.GetComponent<RangedAttack>();
        skill.BonusMultiplier += value * 0.01f;
    }
    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.StatesGO.GetComponent<RangedAttack>().BonusMultiplier -= value * 0.01f;
    }
}