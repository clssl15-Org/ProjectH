using System;
using UnityEngine;

internal class StandaloneHitBrain
{
    public bool IsDamaging { get; private set; } = false;
    private readonly Monster Owner;


    public StandaloneHitBrain(Monster owner)
    {
        Owner = owner;
    }

    public bool TryTakeDamage(int damage)
    {
        if (IsDamaging) return false;
        if (!Owner.IsAlive) return false;
        if (damage < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(damage),
                $"Damage는 0 이상이어야 하지만 '{damage}'이(가) 입력되었습니다.");
        }


        if (!Owner.StandaloneHitAction.TryHit(out var reason, _ => Complete(), playtime: Owner.InvincibleDuration))
        {
            if (reason.Result != ActionResult.ResultType.AlreadyDoing)
                Debug.LogWarning(Owner.Ctx(
                    $"Hit(Standalone) 행동에 실패하였기 때문에 Hit(Standalone) 상태로 진입할 수 없습니다.\n{reason}"));
            
            return false;
        }

        Owner.HP -= damage;
        IsDamaging = true;

        return true;
    }

    public void Stop() => Owner.StandaloneHitAction.StopAction();

    private void Complete()
    {
        IsDamaging = false;
    }

    private string CtxHit(string message) => Owner.Ctx($"StandaloneHit: {message}");
}
