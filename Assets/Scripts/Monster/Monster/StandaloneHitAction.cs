using System;
using UnityEngine;

namespace MonsterActions
{
    [DisallowMultipleComponent]
    internal class StandaloneHitAction : MonoBehaviour
    {
        // Display
        [SerializeField, Header("Display")]
        private string stateDisplay = string.Empty;


        // Internal
        private Monster monster;

        private class StandaloneHitActionController : MonsterActionController
        {
            public StandaloneHitActionController(Monster monster) : base(monster)
            {
                AddChild(new HitFlash());
            }
        }
        private StandaloneHitActionController controller;


        // Content
        public void Initialize(Monster monster)
        {
            this.monster = monster;

            controller = new(monster);
            controller.Enter();
        }

        public bool TryHit(
            out ActionResult reason,
            Action<ActionResult> callback = null,
            bool stopPreviousAction = true,
            bool allowRestart = false,
            float? playtime = null,
            float stayTimeAfterFinished = 0)
        {
            if (controller == null)
                throw new InvalidOperationException(monster.Ctx(
                    $"{GetType().Name} 컴포넌트를 사용하기 전에 Initialize 메서드를 호출하여 컴포넌트를 초기화해야 합니다."));

            return controller.TryDoAction(MonsterAction.Hit.ToString(), out reason, callback, stopPreviousAction, allowRestart, playtime, stayTimeAfterFinished);
        }

        public void StopAction() => controller.StopCurrentAction();

        private void Update()
        {
            controller?.Update();

#if UNITY_EDITOR
            UpdateDisplayConetnt();
#endif
        }

        private void UpdateDisplayConetnt()
        {
            if (controller == null)
                stateDisplay = "Not Initialized";
            else
                stateDisplay = controller.TryGetCurrentAction(out var name) ? name : "None";
        }

        private void OnDestroy() => controller?.Dispose();
    }
}
