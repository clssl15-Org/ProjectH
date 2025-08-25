using UnityEngine;
using MonsterActions;

public partial class Dokkaebi : Monster
{
    private class AttackAction : MonsteActionState
    {
        private GameObject laser;


        public AttackAction() : base(MonsterAction.Attack.ToString()) { }

        protected override void OnEnter(params object[] inputs)
        {
            var owner = (Dokkaebi)Owner;

            laser = Instantiate(owner.laserPrefab);
            laser.transform.SetParent(owner.transform);

            laser.transform.localPosition = owner.laserPosition;
            laser.transform.localScale = Vector3.one;

            base.OnEnter(inputs);
        }

        protected override void OnExit()
        {
            if (laser)
            {
                Destroy(laser);
                laser = null;
            }

            base.OnExit();
        }
    }
}
