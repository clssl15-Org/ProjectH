using UnityEngine;
using Actors.Monsters.Actions;

namespace Actors.Monsters.Brains
{
    internal class StandaloneHitBrain
    {
        public bool DoKnockback { get; set; }
        public bool IsDamaging { get; private set; } = false;

        private readonly IMonsterInternal _owner;
        private MonsterConditionData _notification;


        public StandaloneHitBrain(IMonsterInternal owner, bool doKnockback = true)
        {
            _owner = owner;
            DoKnockback = doKnockback;
        }

        public bool TryTakeDamage(DamageInfo damageInfo)
        {
            if (IsDamaging) return false;
            if (!_owner.IsAlive) return false;

            IsDamaging = true;

            _owner.HP -= damageInfo.Damage;

            _notification = new MonsterConditionData(MonsterCondition.Damaged, damageInfo);
            _owner.NotifyCondition(_notification);

            if (DoKnockback && damageInfo.HasKnockback)
                _owner.Knockback(damageInfo.Direction, damageInfo.KnockbackForce);

            if (!_owner.StandaloneHitAction.TryHit(out var reason, _ => Complete(), playTime: _owner.StatsInfo.InvincibleDuration))
            {
                if (reason.ResultType != ResultType.AlreadyDoing)
                    Debug.LogWarning(CtxHit(
                        $"Hit 행동에 실패하였기 때문에 Hit 상태로 진입할 수 없습니다.\n{reason}"));

                Complete();
                return false;
            }

            return true;
        }

        public void Stop() => _owner.StandaloneHitAction.StopAction();

        private void Complete()
        {
            _notification?.Complete();
            _notification = null;

            IsDamaging = false;
        }

        private string CtxHit(string message) => _owner.FormatLogMessage($"StandaloneHit: {message}");
    }
}
