using System;
using Actors;
using UnityEngine;

namespace UI.Monsters
{
    public class MonsterVM : IMonsterVM
    {
        // Front
        public Vector2 WorldPosition => _monster?.transform.position ?? Vector2.zero;
        public HealthRateData HealthRate => new(_monster.HP, _monster.MaxHP);

        public event Action<HealthRateData> HealthRateChanged;
        public event Action Disposed;

        public bool IsDisposed { get; private set; } = false;

        // Internal
        private IMonster _monster;

        private UILibrary _uILibrary;
        private Action<IView> _viewRegisterer;


        // Content
        public MonsterVM(IMonster monster, UILibrary uILibrary, Action<IView> viewRegisterer)
        {
            #region null 체크
            if (monster == null)
                throw new ArgumentNullException(nameof(monster), Ctx(
                    "입력 인자는 null일 수 없습니다."));
            if (uILibrary == null)
                throw new ArgumentNullException(nameof(uILibrary), Ctx(
                    "입력 인자는 null일 수 없습니다."));
            if (viewRegisterer == null)
                throw new ArgumentNullException(nameof(viewRegisterer), Ctx(
                    "입력 인자는 null일 수 없습니다."));
            #endregion

            _monster = monster;
            _monster.ConditionChanged += Update;
            _monster.Destroyed += Dispose;

            _uILibrary = uILibrary;
            _viewRegisterer = viewRegisterer;

            var healthBar = _uILibrary.HealthBar;
            healthBar.Connect(this);

            _viewRegisterer(healthBar);
        }

        private void Update(IMonsterConditionData condition)
        {
            if (condition.Is(MonsterCondition.Heal, MonsterCondition.Damaged))
                HealthRateChanged?.Invoke(HealthRate);
        }

        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            Disposed?.Invoke();

            _monster.ConditionChanged -= Update;
            _monster.Destroyed -= Dispose;

            HealthRateChanged = null;
            Disposed = null;
            _monster = null;
            _viewRegisterer = null;
        }

        private string Ctx(string message) => $"[{nameof(MonsterVM)}]: {message}";
    }
}
