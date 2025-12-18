using UnityEngine;

namespace Actors.Monsters.Actions
{
    internal class AttackWithWeapon : MonsterActionComponent
    {
        // Internal
        private Weapon _weaponPrefab;
        private float _startTime;
        private float _duration;

        private Weapon _weapon;
        private float _elapsedTime;
        private int _phase;


        // Content
        [System.Obsolete]
        public AttackWithWeapon(GameObject weaponPrefab, float startTime = 0, float duration = float.MaxValue) =>
            throw new System.NotImplementedException("이 생성자는 더 이상 사용되지 않습니다. 대신 Weapon 타입을 사용하는 생성자를 사용하세요.");

        public AttackWithWeapon(Weapon weaponPrefab, float startTime = 0, float? duration = null)
        {
            _weaponPrefab = weaponPrefab;
            _startTime = startTime;
            _duration = duration ?? float.MaxValue;
        }

        protected override void OnEnter(object _)
        {
            if (!_weaponPrefab)
            {
                Debug.LogWarning(
                    $"[{nameof(AttackWithWeapon)}] {nameof(_weaponPrefab)}이(가) 유효하지 않으므로 컴포넌트가 비활성화되었습니다.",
                    Owner.gameObject);

                Interrupt(InterruptType.Completed);
                return;
            }

            _elapsedTime = 0;
            _phase = 0;

            if (_startTime <= 0)
            {
                _phase = 1;
                SetWeapon();
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_phase >= 2)
                return;

            _elapsedTime += deltaTime;

            if (_phase == 0 && _elapsedTime >= _startTime)
            {
                _phase = 1;
                SetWeapon();
            }

            if (_phase == 1 && _elapsedTime >= _startTime + _duration)
            {
                _phase = 2;
                UnsetWeapon();
            }
        }

        private void SetWeapon()
        {
            _weapon = Object.Instantiate(_weaponPrefab);
            _weapon.transform.SetParent(Owner.transform);

            _weapon.transform.SetPositionAndRotation(_weaponPrefab.transform.position, _weapon.transform.rotation);
            _weapon.transform.localScale = _weaponPrefab.transform.localScale;

            _weapon.gameObject.SetActive(true);
        }

        private void UnsetWeapon()
        {
            if (_weapon) Object.Destroy(_weapon.gameObject);
            _weapon = null;
        }
        
        protected override void OnInterrupt(InterruptType _)
        {
            UnsetWeapon();
        }
    }
}
