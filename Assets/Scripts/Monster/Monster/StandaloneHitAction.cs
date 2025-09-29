using System;
using UnityEngine;

namespace MonsterActions
{
    [DisallowMultipleComponent]
    internal class StandaloneHitAction : MonoBehaviour
    {
        // Display
        [SerializeField, Header("Display"), TextArea(3, 15)]
        private string _stateDisplay = string.Empty;

        // Internal
        private Monster monster;

        private Material originalMaterial;
        private bool materialRestored;

        private Action<ActionResult> callback;
        private bool isRunning = false;
        private bool succeeded = false;

        private float? mainAnimationLength;
        private float? mainAnimationRemainingTime;


        // Content
        public void Initialize(Monster monster) => this.monster = monster;

        public bool TryHit(
            out ActionResult reason,
            Action<ActionResult> callback = null,
            bool allowRestart = false,
            float? playtime = null)
        {
            if (!monster)
            {
                reason = new(ActionResult.ResultType.InvalidOperation,
                    Ctx($"{nameof(monster)} 컴포넌트({monster?.name ?? "Null"})가 유효하지 않습니다."));

                return false;
            }

            if (!allowRestart && isRunning)
            {
                reason = new(ActionResult.ResultType.AlreadyDoing,
                    Ctx($"이미 Hit 행동을 실행하고 있기 때문에 행동을 재실행할 수 없습니다."));

                return false;
            }

            this.callback = callback;

            mainAnimationLength = playtime.HasValue
                ? (playtime.Value >= 0 ? playtime.Value : null)
                : monster.InvincibleDuration;

            mainAnimationRemainingTime = mainAnimationLength;

            originalMaterial = monster.SpriteRenderer.material;
            monster.SpriteRenderer.material = monster.SceneAssetsLibrary.SolidColor;
            monster.SpriteRenderer.material.color = Color.white;

            isRunning = true;
            succeeded = false;
            materialRestored = false;

            reason = new(ActionResult.ResultType.Success);
            return true;
        }

        private void Update()
        {
            DoUpdate();

#if UNITY_EDITOR
            UpdateDisplayConetnt();
#endif
        }

        private void DoUpdate()
        {
            if (!isRunning)
                return;

            if (!mainAnimationRemainingTime.HasValue)
                return;

            if (!monster.SpriteRenderer)
            {
                isRunning = false;
                return;
            }


            mainAnimationRemainingTime -= Time.deltaTime;

            if (mainAnimationRemainingTime <= mainAnimationLength - monster.DamageFlashDuration)
                RestoreMaterial();

            if (mainAnimationRemainingTime <= 0)
            {
                succeeded = true;
                StopAction();
            }
        }

        public void StopAction()
        {
            RestoreMaterial();
            isRunning = false;

            callback?.Invoke(new(succeeded
                ? ActionResult.ResultType.Success
                : ActionResult.ResultType.Interrupted));
        }

        private void RestoreMaterial()
        {
            if (materialRestored)
                return;

            if (monster && monster.SpriteRenderer)
            {
                monster.SpriteRenderer.material = originalMaterial;
                materialRestored = true;
            }
        }


        private void UpdateDisplayConetnt()
        {
            if (monster == null)
                _stateDisplay = $"Invalid {nameof(monster)} ({monster?.name ?? "Null"})";
            else
                _stateDisplay = $"Running: {isRunning}\nSucceed: {succeeded}";
        }

        private string Ctx(string message)
        {
            if (monster)
                return monster.Ctx(_Ctx(message));
            else
                return _Ctx(message);

            string _Ctx(string message) => $"{GetType().Name}: {message}";
        }
    }
}
