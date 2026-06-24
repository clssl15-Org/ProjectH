using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Actors.PlayerSystem;

public class BrassGearOfFate : Relic
{
    private int[] originProb;
    public override void OnAcquire()
    {
        Player player = RelicManager.Instance.player;
        if (player != null)
        {
            DamageRoulette roulette = player.GetComponent<DamageRoulette>();
            if (roulette != null && roulette.Probabilities != null)
            {
                originProb = (int[])roulette.Probabilities.Clone();

                int[] newProb = (int[])originProb.Clone();
                newProb[0] -= (int)value;
                newProb[1] -= (int)value;
                newProb[2] -= (int)value;
                newProb[3] += (int)value;
                newProb[4] += (int)value;
                newProb[5] += (int)value;

                roulette.Probabilities = newProb;
            }
        }
    }
    protected override void OnLoseCore()
    {
        Player player = RelicManager.Instance?.player;
        if (player != null)
        {
            DamageRoulette roulette = player.GetComponent<DamageRoulette>();
            if (roulette != null && originProb != null)
            {
                roulette.Probabilities = originProb;
            }
        }
    }
}
