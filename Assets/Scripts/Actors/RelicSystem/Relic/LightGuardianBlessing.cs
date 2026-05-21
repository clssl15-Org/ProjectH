using UnityEngine;

/// <summary>
/// 몬스터 처치 시 체력 회복. 실제 처리는 <see cref="RelicManager"/>의 사망 이벤트 구독에서 수행합니다.
/// </summary>
public class LightGuardianBlessing : Relic
{
    public override void OnAcquire() { }

    protected override void OnLoseCore() { }
}
