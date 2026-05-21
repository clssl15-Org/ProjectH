using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BrassGearOfFate : Relic
{
    private int[] originProb;
    public override void OnAcquire()
    {
        originProb = RelicManager.Instance.player.GetComponent<DamageRoulette>().Probabilities;

        int[] newProb = originProb;
        newProb[0] -= (int)value;
        newProb[1] -= (int)value;
        newProb[2] -= (int)value;
        newProb[3] += (int)value;
        newProb[4] += (int)value;
        newProb[5] += (int)value;

        RelicManager.Instance.player.GetComponent<DamageRoulette>().Probabilities = newProb;
    }
    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.GetComponent<DamageRoulette>().Probabilities = originProb;
    }
}
