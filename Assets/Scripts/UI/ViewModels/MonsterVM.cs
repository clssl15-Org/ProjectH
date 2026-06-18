using System;
using System.Linq;
using Actors;
using BlackThunder.BlackboxSystem;
using UnityEngine;

namespace UI
{
    public class MonsterVM : IPositionedVM, IHealthRateVM
    {
        // Front
        public Vector2 WorldPosition
        {
            get
            {
                ThrowIfDisposed();
                return _monster.transform.position;
            }
        }

        public Vector2 WorldBottomPosition
        {
            get
            {
                ThrowIfDisposed();
                return _collider
                    ? new Vector2(
                        _monster.transform.position.x,
                        _collider.bounds.min.y)
                    : WorldPosition;
            }
        }

        public HealthRateData HealthRate
        {
            get
            {
                ThrowIfDisposed();
                return new(_monster.HP, _monster.MaxHP);
            }
        }

        public event Action<HealthRateData> HealthRateChanged;
        public event Action Dead;
        public event Action Disposed;
        public bool IsDisposed { get; private set; } = false;

        // Internal
        private IMonster _monster;
        private Collider2D _collider;
        private BlackboxHandle _blackbox;


        // Content
        public MonsterVM(IMonster monster)
        {
            using var _ = BlackboxHandle.Of(this).Construct("몬스터 ViewModel 생성을 시작합니다.", out _blackbox);

            if (monster == null)
                throw new ArgumentNullException(
                    nameof(monster),
                    Ctx("입력 인자는 null일 수 없습니다."));

            _monster = monster;
            _collider = monster.gameObject.GetComponent<Collider2D>();

            _monster.ConditionChanged += Update;
            _monster.Destroyed += Dispose;
        }

        private void Update(MonsterConditionData condition)
        {
            ThrowIfDisposed();

            if (condition.Is(MonsterCondition.Heal, MonsterCondition.Damaged))
                HealthRateChanged?.Invoke(HealthRate);
            if (condition.Is(MonsterCondition.Dying))
                Dead?.Invoke();
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            using var _ = _blackbox.Scope("몬스터 ViewModel을 정리합니다.");

            IsDisposed = true;

            _monster.ConditionChanged -= Update;
            _monster.Destroyed -= Dispose;

            if (Disposed != null)
                foreach (Action disposed in Disposed.GetInvocationList())
                {
                    if (Disposed == null)
                        break;

                    if (Disposed.GetInvocationList().Contains(disposed))
                        disposed();
                }

            HealthRateChanged = null;
            Dead = null;
            Disposed = null;

            _monster = null;
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
