using System;
using System.Collections.Generic;
using Actors;

namespace UI
{
    public class PlayerVM : IHealthRateVM
    {
        public HealthRateData HealthRate
        {
            get
            {
                ThrowIfDisposed();
                return new(_player.HP, _player.MaxHP);
            }
        }
        public event Action<HealthRateData> HealthRateChanged;

        public IReadOnlyList<int> Relics => _player?.Relics;
        public event Action<int> RelicAcquired;
        public event Action<int> RelicAbandoned;

        public event Action Disposed;
        public bool IsDisposed { get; private set; } = false;

        // Internal
        private IPlayer _player;


        // Content
        public PlayerVM(IPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(
                    nameof(player),
                    Ctx("입력 인자는 null일 수 없습니다."));

            _player = player;

            _player.ConditionChanged += Update;

            // TODO: ConditionChanged에 Payload 담아서 보내기
            _player.RelicAcquired += id => RelicAcquired?.Invoke(id);
            _player.RelicAbandoned += id => RelicAbandoned?.Invoke(id);

            _player.Destroyed += Dispose;
        }

        private void Update(PlayerCondition condition)
        {
            ThrowIfDisposed();

            if (condition == PlayerCondition.Damage
                || condition == PlayerCondition.Heal)
                HealthRateChanged?.Invoke(HealthRate);
        }

        #region Skill Inputs
        public void ChangeSkill(int skillIndex)
        {
            ThrowIfDisposed();
            if (_player == null) return;

            _player.ChangeSkill(skillIndex);
        }
        public void ApplyRandomSkillBuff(float factor)
        {
            _player.ApplyRandomSkillBuff(factor);
        }

        public void UseSkill()
        {
            //ThrowIfDisposed();
            //if (_player == null) return;

            //_player.UseSkill();
        }
        public void DefaultAttack()
        {
            //ThrowIfDisposed();
            //if (_player == null) return;

            //_player.DefaultAttack();
        }
        public void RangedAttack()
        {
            //ThrowIfDisposed();
            //if (_player == null) return;

            //_player.RangedAttack();
        }
        #endregion

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            _player.ConditionChanged -= Update;
            _player.Destroyed -= Dispose;

            Disposed?.Invoke();
            Disposed = null;

            HealthRateChanged = null;
            _player = null;
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(
                    Ctx("이미 Dispose된 객체에 접근하려고 시도했습니다."));
        }

        private string Ctx(string message) => $"[{nameof(MonsterVM)}] {message}";
    }
}
