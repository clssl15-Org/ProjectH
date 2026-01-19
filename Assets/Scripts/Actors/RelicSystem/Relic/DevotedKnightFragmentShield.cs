using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DevotedKnightFragmentShield : Relic
{
    public override void OnAcquire()
    {
        OnReinforcedAcquire();

        RelicManager.Instance.player.sieldCount += (int)value;
        RelicManager.Instance.player.PlayerHealth.OnSieldBreak += OnLose;
    }

    public override void OnLose()
    {
        // 가지고 있는 렐릭 리스트에서 제거
    }
}
