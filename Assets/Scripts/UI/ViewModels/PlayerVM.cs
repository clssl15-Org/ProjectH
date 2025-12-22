using System;
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
            _player.Destroyed += Dispose;
        }

        private void Update(PlayerCondition condition)
        {
            ThrowIfDisposed();

            if (condition == PlayerCondition.Damage
                || condition == PlayerCondition.Heal)
                HealthRateChanged?.Invoke(HealthRate);
        }

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
