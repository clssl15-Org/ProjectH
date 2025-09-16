using UnityEngine;

namespace MonsterActions
{
    internal class AttackWithWeapon : MonsteActionState
    {
        // Internal
        private GameObject weaponPrefab;
        private float startTime;

        private GameObject weapon;
        private float playtime;
        private bool weaponSetted;


        // Content
        public AttackWithWeapon(GameObject weaponPrefab, float startTime = 0, MonsterAction monsterAction = MonsterAction.Attack) : this(weaponPrefab, startTime, monsterAction.ToString()) { }
        public AttackWithWeapon(GameObject weaponPrefab, float startTime, string monsterAction) : base(monsterAction)
        {
            this.weaponPrefab = weaponPrefab;
            this.startTime = startTime;
        }

        protected override void OnEnter(params object[] inputs)
        {
            playtime = 0;
            weaponSetted = false;

            if (startTime <= 0)
                SetWeapon();

            base.OnEnter(inputs);
        }

        protected override void OnUpdate()
        {
            if (weaponSetted)
                return;

            playtime += Time.deltaTime;

            if (playtime >= startTime)
                SetWeapon();
        }

        private void SetWeapon()
        {
            weaponSetted = true;

            weapon = Object.Instantiate(weaponPrefab);
            weapon.transform.SetParent(Owner.transform);

            weapon.transform.SetPositionAndRotation(weaponPrefab.transform.position, weapon.transform.rotation);
            weapon.transform.localScale = weaponPrefab.transform.localScale;

            weapon.SetActive(true);
        }

        protected override void OnExit()
        {
            if (weapon)
            {
                Object.Destroy(weapon);
                weapon = null;
            }

            base.OnExit();
        }
    }
}
