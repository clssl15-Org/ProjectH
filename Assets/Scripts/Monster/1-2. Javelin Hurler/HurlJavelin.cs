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
        protected override void OnEnter(object input)
        {
            _isJavelinThrown = false;
        }
        
        protected override void OnUpdate(float elapsedTime)
        {
            if (!_isJavelinThrown && elapsedTime >= JavelinHurler._throwTime)
            {
                _isJavelinThrown = true;

                var javelin = Instantiate(JavelinHurler._javelinPrefab).GetComponent<Javelin>();
                javelin.Initialize(JavelinHurler.PlatformManager, "Ground");

                javelin.transform.SetParent(JavelinHurler.transform);
                javelin.transform.localPosition = JavelinHurler._javelinPrefab.transform.localPosition;
                javelin.transform.localScale = JavelinHurler._javelinPrefab.transform.localScale;
                javelin.gameObject.SetActive(true);

                var flipped = javelin.transform.lossyScale.x < 0;

                javelin.Throw(
                    Quaternion.Euler(0, 0, flipped ? JavelinHurler._throwAngle : 180 - JavelinHurler._throwAngle),
                    JavelinHurler._throwPower);

                Interrupt(InterruptType.Completed);
            }
        }
    }
}
