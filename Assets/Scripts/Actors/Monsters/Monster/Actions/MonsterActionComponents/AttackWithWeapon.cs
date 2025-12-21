using UnityEngine;

namespace Actors.Monsters.Actions
{
    internal class AttackWithWeapon : MonsterActionComponent
    {
        // Internal
        private IWeapon _weaponPrefab;
        private float _startTime;
        private float _duration;

        private IWeapon _weapon;
        private float _elapsedTime;
        private int _phase;


        // Content
        public AttackWithWeapon(IWeapon weaponPrefab, float startTime = 0, float duration = -1)
        {
            _weaponPrefab = weaponPrefab;
            _startTime = startTime;
            _duration = duration < 0 ? float.MaxValue : duration;
        }

        protected override void OnEnter(object _)
        {
            if (_weaponPrefab == null)
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
            _weapon = Object
                .Instantiate(_weaponPrefab.gameObject)
                .GetComponent<IWeapon>();
            _weapon.transform.SetParent(Owner.transform);

            _weapon.transform.SetPositionAndRotation(_weaponPrefab.transform.position, _weapon.transform.rotation);
            _weapon.transform.localScale = _weaponPrefab.transform.localScale;

            _weapon.gameObject.SetActive(true);
        }

        private void UnsetWeapon()
        {
            if (_weapon != null)
            {
                try
                {
                    Object.Destroy(_weapon.gameObject);
                }
                catch { }
            }

            _weapon = null;
        }
        
        protected override void OnInterrupt(InterruptType _)
        {
            UnsetWeapon();
        }
    }
}
