using UnityEngine;
using MonsterActions;

public partial class JavelinHurler
{
    private class JavelinHurlerAttackAction : MonsterAction
    {
        // Internal
        private float totalPlaytime;
        private bool thrown;


        // Content
        public JavelinHurlerAttackAction() : base(MonsterActionType.Attack.ToString()) { }

        protected override void OnEnter(params object[] inputs)
        {
            thrown = false;
            base.OnEnter(inputs);

            if (!MainAnimationRemainingTime.HasValue)
                throw new System.InvalidOperationException("창던지개의 Attack 행동은 종료 시간이 존재해야 합니다.");

            totalPlaytime = MainAnimationRemainingTime.Value;
        }
        
        protected override void OnUpdate()
        {
            base.OnUpdate();
            var owner = (JavelinHurler)Owner;

            if (!thrown && (totalPlaytime - MainAnimationRemainingTime) >= owner.throwTime)
            {
                thrown = true;

                var javelin = Instantiate(owner.javelinPrefab).GetComponent<Javelin>();
                javelin.Initialize(owner.PlatformManager, "Ground");

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
