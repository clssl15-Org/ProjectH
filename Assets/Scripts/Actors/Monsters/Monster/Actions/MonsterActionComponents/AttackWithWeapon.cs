using UnityEngine;

namespace Actors.Monsters.Actions
{
    internal class AttackWithWeapon : MonsterActionComponent
    {
        // Internal
        private GameObject _weaponPrefab;
        private float _startTime;
        private float _duration;

        private GameObject _weapon;
        private int _phase;


        // Content
        public AttackWithWeapon(GameObject weaponPrefab, float startTime = 0, float duration = float.MaxValue)
        {
            _weaponPrefab = weaponPrefab;
            _startTime = startTime;
            _duration = duration;
        }

        protected override void OnEnter(float _, object __)
        {
            if (!_weaponPrefab)
            {
                Debug.LogWarning(
                    $"[{nameof(AttackWithWeapon)}] {nameof(_weaponPrefab)}이(가) 유효하지 않으므로 컴포넌트가 비활성화되었습니다.",
                    Owner.gameObject);

                Interrupt(InterruptType.Completed);
                return;
            }

            _phase = 0;

            if (_startTime <= 0)
            {
                _phase = 1;
                SetWeapon();
            }
        }

        protected override void OnUpdate(float elapsedTime)
        {
            if (_phase >= 2)
                return;

            if (_phase == 0 && elapsedTime >= _startTime)
            {
                _phase = 1;
                SetWeapon();
            }

            if (_phase == 1 && elapsedTime >= _startTime + _duration)
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

            _weapon.SetActive(true);
        }

        private void UnsetWeapon()
        {
            if (_weapon)
            {
                Object.Destroy(_weapon);
                _weapon = null;
            }
        }

        protected override void OnInterrupt(InterruptType _)
        {
            UnsetWeapon();
        }
    }
}
