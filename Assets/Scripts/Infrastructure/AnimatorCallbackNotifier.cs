using UnityEngine;

namespace Infrastructure
{
    public delegate void AnimatorCallbackDelegate(AnimatorStateInfo stateInfo, bool isEnter);


    public class AnimatorCallbackNotifier : StateMachineBehaviour
    {
        public event AnimatorCallbackDelegate Callback;


        public override void OnStateEnter(Animator _, AnimatorStateInfo stateInfo, int __)
        {
            Callback?.Invoke(stateInfo, true);
        }

        public override void OnStateExit(Animator _, AnimatorStateInfo stateInfo, int __)
        {
            Callback?.Invoke(stateInfo, false);
        }
    }
}
