using UnityEngine;

namespace Actors.Monsters
{
    public class WeaponManager : MonoBehaviour, IWeapon
    {
        public int AttackPower
        {
            get => _attackPower;
            set
            {
                _attackPower = value;

                foreach (var weapon in _weapons)
                    weapon.AttackPower = value;
            }
        }

        public bool DoKnockback
        {
            get => _doKnockback;
            set
            {
                _doKnockback = value;

                foreach (var weapon in _weapons)
                    weapon.DoKnockback = value;
            }
        }

        public float? KnockbackForce
        {
            get => _knockbackForce;
            set
            {
                _knockbackForce = value;

                foreach (var weapon in _weapons)
                    weapon.KnockbackForce = value;
            }
        }


        [SerializeField] private Weapon[] _weapons;
        [Space]
        [SerializeField] private bool _applyPropertiesOnAwake = true;
        [SerializeField, Min(0)] private int _attackPower = 1;
        [SerializeField] private bool _doKnockback = true;
        private float? _knockbackForce = null;
        

        private void Awake()
        {
            if (_applyPropertiesOnAwake)
            {
                AttackPower = _attackPower;
                DoKnockback = _doKnockback;
                KnockbackForce = _knockbackForce;
            }
        }
    }
}
