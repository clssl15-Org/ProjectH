using System;
using System.Linq;
using System.Collections.Generic;
using Actors;
using Actors.PlayerSystem;
using Infrastructure;

namespace UI
{
    public class PlayerVM : IHealthRateVM
    {
        public HealthRateData HealthRate
        {
            get
            {
                ThrowIfDisposed();
                return new(_player.HP, _player.MaxHP, _player.BaseMaxHP);
            }
        }
        public event Action<HealthRateData> HealthRateChanged;

        public IEnumerable<SkillType> HavingSkills
        {
            get
            {
                ThrowIfDisposed();
                return _player.HavingSkills;
            }
        }
        public SkillType SelectedSkillType
        {
            get
            {
                ThrowIfDisposed();
                return _player.SelectedSkillType;
            }
        }
        public event Action<SkillType> SkillAdded;
        public event Action<SkillType> SkillChanged;
        public event Action<float> SkillRouletteApplied;
        public event Action SkillRouletteCleared;

        public float CurrentSkillCooldown
        {
            get
            {
                ThrowIfDisposed();
                return _player.CurrentSkillCooldown;
            }
        }
        public float CurrentUltimateCooldown
        {
            get
            {
                ThrowIfDisposed();
                return _player.CurrentUltimateCooldown;
            }
        }

        public event Action<Func<float>> CooltimeEnabled;
        public event Action CooltimeDisabled;

        public event Action Disposed;
        public bool IsDisposed { get; private set; } = false;

        // Internal
        private IPlayer _player;

        private IDisposable _updater;
        private bool? _isSkillCooltime = null;


        // Content
        public PlayerVM(IPlayer player)
        {
            if (player == null)
                throw new ArgumentNullException(
                    nameof(player),
                    Ctx("입력 인자는 null일 수 없습니다."));

            _player = player;
   
            _player.ConditionChanged += OnHealthUpdated;
            _player.SkillAdded += OnSkillAdded;
            _player.SkillChanged += OnSkillChanged;
            _player.SkillRouletteApplied += OnSkillRouletteApplied;
            _player.SkillRouletteCleared += OnSkillRouletteCleared;
            _player.Destroying += Dispose;

            _updater = Loco.Subscribe(Update);
        }

        private void OnHealthUpdated(PlayerCondition condition)
        {
            ThrowIfDisposed();

            if (condition == PlayerCondition.Damage
                || condition == PlayerCondition.Heal
                || condition == PlayerCondition.MaxHealthChanged)
            {
                HealthRateChanged?.Invoke(HealthRate);
            }
        }
        private void OnSkillChanged(SkillType skillType)
        {
            ThrowIfDisposed();

            if (skillType == SkillType.RushStabbing
                || skillType == SkillType.StrongAttack
                || skillType == SkillType.Skill3)
            {
                SkillChanged?.Invoke(skillType);
            }
        }
        private void OnSkillAdded(SkillType skillType)
        {
            ThrowIfDisposed();

            if (skillType == SkillType.RushStabbing
                || skillType == SkillType.StrongAttack
                || skillType == SkillType.Skill3)
            {
                SkillAdded?.Invoke(skillType);
            }
        }

        private void OnSkillRouletteApplied(float bonus)
        {
            ThrowIfDisposed();
            SkillRouletteApplied?.Invoke(bonus);
        }

        private void OnSkillRouletteCleared()
        {
            ThrowIfDisposed();
            SkillRouletteCleared?.Invoke();
        }


        #region Skill Inputs
        public void ChangeSkill()
        {
            ThrowIfDisposed();
            if (_player == null) return;

            _player.ChangeSkill();
        }
        public bool TrySkillRoulette(out float appliedBonus, out Action apply)
        {
            ThrowIfDisposed();

            if (_player == null
                || !_player.TrySkillRoulette(out var rouletteCtx))
            {
                (appliedBonus, apply) = (default, default);
                return false;
            }

            float selector = UnityEngine.Random.Range(0f, rouletteCtx.Probabilities.Sum());
            float criteria = 0f;

            foreach (var (bouns, prob)
                in rouletteCtx.Bonuses.Zip(rouletteCtx.Probabilities, (bouns, prob) => (bouns, prob)))
            {
                criteria += prob;
                if (selector < criteria)
                {
                    appliedBonus = bouns;
                    apply = () => rouletteCtx.Apply(bouns);
                    return true;
                }
            }

            (appliedBonus, apply) = (default, default);
            return false;
        }

        private void Update()
        {
            if (_player == null) return;

            var isSkillCooltime = _player.CurrentSkillCooldown >= 0;
            if (isSkillCooltime == _isSkillCooltime) return;

            _isSkillCooltime = isSkillCooltime;

            if (isSkillCooltime)
                CooltimeEnabled?.Invoke(() => _player.CurrentSkillCooldown);
            else
                CooltimeDisabled?.Invoke();
        }

        [Obsolete("현재 Player Input은 Standalone으로 처리됩니다.")]
        public void UseSkill()
        {
            //ThrowIfDisposed();
            //if (_player == null) return;

            //_player.UseSkill();
        }
        [Obsolete("현재 Player Input은 Standalone으로 처리됩니다.")]
        public void DefaultAttack()
        {
            //ThrowIfDisposed();
            //if (_player == null) return;

            //_player.DefaultAttack();
        }
        [Obsolete("현재 Player Input은 Standalone으로 처리됩니다.")]
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

            _updater?.Dispose();
            _updater = null;

            if (_player != null)
            {
                _player.ConditionChanged -= OnHealthUpdated;
                _player.SkillAdded -= OnSkillAdded;
                _player.SkillChanged -= OnSkillChanged;
                _player.SkillRouletteApplied -= OnSkillRouletteApplied;
                _player.SkillRouletteCleared -= OnSkillRouletteCleared;
                _player.Destroying -= Dispose;

                _player = null;
            }

            Disposed?.Invoke();
            Disposed = null;
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
                throw new ObjectDisposedException(
                    Ctx("이미 Dispose된 객체에 접근하려고 시도했습니다."));
        }

        private string Ctx(string message) => $"[{nameof(PlayerVM)}] {message}";
    }
}
