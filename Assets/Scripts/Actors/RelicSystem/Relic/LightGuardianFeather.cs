using UnityEngine;

public class LightGuardianFeather : Relic
{
    public override void OnAcquire()
    {
        RelicManager.Instance.player.playerStats.maxDashCount += isReinforced ? 2 : 1;
    }

    protected override void OnLoseCore()
    {
        RelicManager.Instance.player.playerStats.maxDashCount -= isReinforced ? 2 : 1;
    }
}