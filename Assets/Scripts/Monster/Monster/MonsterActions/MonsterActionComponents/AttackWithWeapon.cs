using UnityEngine;

namespace MonsterActions
{
    internal class AttackWithWeapon : MonsterActionComponent
    {
        // Internal
        private GameObject _weaponPrefab;
        private float _startTime;

        private GameObject _weapon;
        private bool _isWeaponSetted;


        // Content
        public AttackWithWeapon(GameObject weaponPrefab, float startTime = 0)
        {
            _weaponPrefab = weaponPrefab;
            _startTime = startTime;
        }

        public override void Enter(object input)
        {
            _isWeaponSetted = false;

            if (_startTime <= 0)
                SetWeapon();

            base.Enter(input);
        }

        public override void Update(float elapsedTime)
        {
            if (_isWeaponSetted)
                return;

            if (elapsedTime >= _startTime)
                SetWeapon();
        }

        private void SetWeapon()
        {
            _isWeaponSetted = true;

            _weapon = Object.Instantiate(_weaponPrefab);
            _weapon.transform.SetParent(Owner.transform);

            _weapon.transform.SetPositionAndRotation(_weaponPrefab.transform.position, _weapon.transform.rotation);
            _weapon.transform.localScale = _weaponPrefab.transform.localScale;

            _weapon.SetActive(true);
        }

        public override void Interrupt(InterruptType reason)
        {
            if (_weapon)
            {
                Object.Destroy(_weapon);
                _weapon = null;
            }

            base.Interrupt(reason);
        }
    }
}
