using UnityEngine;

internal class StandaloneHitBrain
{
    public bool DoKnockback { get; set; }
    public bool IsDamaging { get; private set; } = false;

    private readonly IMonster _owner;


    public StandaloneHitBrain(IMonster owner, bool doKnockback = true)
    {
        _owner = owner;
        DoKnockback = doKnockback;
    }

    public bool TryTakeDamage(DamageInfo damageInfo)
    {
        if (IsDamaging) return false;
        if (!_owner.IsAlive) return false;

        if (!_owner.StandaloneHitAction.TryHit(out var reason, _ => Complete(), playTime: _owner.StatsInfo.InvincibleDuration))
        {
            if (reason.Result != ActionResult.ResultType.AlreadyDoing)
                Debug.LogWarning(_owner.FormatLogMessage(
                    $"Hit(Standalone) 행동에 실패하였기 때문에 Hit(Standalone) 상태로 진입할 수 없습니다.\n{reason}"));
            
            return false;
        }

        _owner.HP -= damageInfo.Damage;

        if (DoKnockback && damageInfo.HasKnockback)
            _owner.Knockback(damageInfo.Direction, damageInfo.KnockbackForce);


        IsDamaging = true;
        return true;
    }

    public void Stop() => _owner.StandaloneHitAction.StopAction();

    private void Complete()
    {
        IsDamaging = false;
    }

    private string CtxHit(string message) => _owner.FormatLogMessage($"StandaloneHit: {message}");
}
