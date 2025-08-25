using UnityEngine;
using MonsterActions;

public partial class JavelinHurler
{
    private class AttackAction : MonsteActionState
    {
        // Internal
        private float playtime;
        private bool thrown;


        // Content
        public AttackAction() : base(MonsterAction.Attack.ToString()) { }

        protected override void OnEnter(params object[] inputs)
        {
            thrown = false;
            base.OnEnter(inputs);

            if (!RemainingTime.HasValue)
                throw new System.InvalidOperationException("창던지개의 Attack 행동은 종료 시간이 존재해야 합니다.");

            playtime = RemainingTime.Value;
        }
        
        protected override void OnUpdate()
        {
            base.OnUpdate();
            var owner = (JavelinHurler)Owner;

            if (!thrown && (playtime - RemainingTime) >= owner.throwTime)
            {
                thrown = true;

                var javelin = Instantiate(owner.javelinPrefab).GetComponent<Javelin>();
                javelin.platformManager = owner.platformManager;

                javelin.transform.SetParent(owner.transform);
                javelin.transform.localPosition = owner.javelinPosition;
                javelin.transform.localScale = owner.javelinScale * Vector3.one;

                var flipped = javelin.transform.lossyScale.x < 0;

                javelin.Throw(
                    Quaternion.Euler(0, 0, flipped ? owner.throwAngle : 180 - owner.throwAngle),
                    owner.throwPower);

            }
        }
    }
}
