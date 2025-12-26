using System;
using Infrastructure;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class BossUI : MonoBehaviour, IView, IEnablable
    {
        [field: SerializeField] public bool DestoyOnMonsterDead { get; set; } = true;
        [SerializeField] private HealthBar _healthBar;
        [SerializeField] private Animation _animation;
        private EnableWithAnimation _enabler;

        public event Action Destroyed;

        #region Interfaces
        Action IEnablable.Enabling => null;
        Action IEnablable.Enabled => null;
        Action IEnablable.Disabling => null;
        Action IEnablable.Disabled => _desabled;
        #endregion

        private bool _awaken = false;
        private MonsterVM _boss;
        private Action _desabled = null;


        // Content
        public void Awake()
        {
            if (_awaken) return;
            _awaken = true;

            if (!_healthBar)
                throw new InvalidOperationException(
                    $"[{nameof(BossUI)}] {nameof(_healthBar)} 필드는 null일 수 없습니다. " +
                    $"인스펙터에서 올바르게 설정되었는지 확인하세요.");

            _enabler = new EnableWithAnimation(_animation)
                .InitializeWithIEnablable(this);
        }

        public void Connect(MonsterVM boss)
        {
            if (boss == null)
                throw new ArgumentNullException(
                    nameof(boss),
                    $"[{nameof(HealthBar)}] 인자는 null일 수 없습니다.");
            if (_boss == boss)
                return;
            if (_boss != null)
                throw new InvalidOperationException(
                    $"[{nameof(HealthBar)}] {nameof(_boss)}이(가) 이미 존재하기 때문에 새로운 연결을 구성할 수 없습니다.");

            _boss = boss;
            _healthBar.Connect(boss);

            if (DestoyOnMonsterDead)
                _boss.Dead += DestoyOnDead;
        }

        public void Disconnect()
        {
            if (_boss == null)
                return;

            _healthBar.Disconnect();
            _boss = null;
        }

        private void DestoyOnDead()
        {
            Disconnect();

            _desabled = Destroy;
            Disable();
        }

        public void Enable()
        {
            Awake();
            _enabler.Enable();
        }
        public void Disable()
        {
            Awake();
            _enabler.Disable();
        }
        public void SetToEnabled()
        {
            Awake();
            _enabler.SetToEnabled();
        }
        public void SetToDisabled()
        {
            Awake();
            _enabler.SetToDisabled();
        }

        public void SetParent(RectTransform parent) =>
            GetComponent<RectTransform>().SetParent(parent);

        public void Destroy()
        {
            Destroyed?.Invoke();

            if (this && gameObject)
                Destroy(gameObject);
        }
    }
}
