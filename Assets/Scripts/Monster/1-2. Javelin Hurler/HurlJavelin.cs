using UnityEngine;
using MonsterActions;

public partial class JavelinHurler
{
    private class HurlJavelin : MonsterActionComponent
    {
        // Internal
        private JavelinHurler JavelinHurler => (JavelinHurler)MonsterAction.Owner;
        private bool _isJavelinThrown;


        // Content
        public override void Enter(object input)
        {
            base.Enter(input);
            _isJavelinThrown = false;
        }
        
        public override void Update(float elapsedTime)
        {
            if (!_isJavelinThrown && elapsedTime >= JavelinHurler.throwTime)
            {
                _isJavelinThrown = true;

                var javelin = Instantiate(JavelinHurler.javelinPrefab).GetComponent<Javelin>();
                javelin.Initialize(JavelinHurler.PlatformManager, "Ground");

                javelin.transform.SetParent(JavelinHurler.transform);
                javelin.transform.localPosition = JavelinHurler.javelinPosition;
                javelin.transform.localScale = JavelinHurler.javelinScale * Vector3.one;

                var flipped = javelin.transform.lossyScale.x < 0;

                javelin.Throw(
                    Quaternion.Euler(0, 0, flipped ? JavelinHurler.throwAngle : 180 - JavelinHurler.throwAngle),
                    JavelinHurler.throwPower);

                Interrupt(InterruptType.Completed);
            }
        }
    }
}
