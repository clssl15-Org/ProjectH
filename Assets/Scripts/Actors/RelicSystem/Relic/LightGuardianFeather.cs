using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightGuardianFeather : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.playerStats.maxDashCount += 1;

        if (value == 2)
        {
            RelicManager.Instance.player.playerStats.canJumpAfterDash = true;
        }
    }

    public override void OnLose()
    {
        RelicManager.Instance.player.playerStats.maxDashCount -= 1;

        if (value == 2)
        {
            RelicManager.Instance.player.playerStats.canJumpAfterDash = false;
        }
    }
}
