using UnityEngine;

namespace Actors.Monsters
{
    public class WeaponManager : MonoBehaviour
    {
        public int AttackPower
        {
            get => _attackPower;
            set
            {
                foreach (var weapon in _weapons)
                    weapon.AttackPower = AttackPower;
            }
        }
        private int _attackPower = 1;

        [SerializeField] private Weapon[] _weapons;
    }
}
