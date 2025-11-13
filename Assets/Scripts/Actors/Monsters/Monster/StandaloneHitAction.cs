using System;
using UnityEngine;

namespace Actors.Monsters.Actions
{
    [DisallowMultipleComponent]
    internal class StandaloneHitAction : MonoBehaviour
    {
        // Display
        [SerializeField, Header("Display"), TextArea(3, 15)]
        private string _stateDisplay = string.Empty;

        // Internal
        private const float DamageFlashDuration = 0.1f;
        private IMonsterInternal _monster;

        private Material _originalMaterial;
        private bool _materialRestored;

        private Action<ActionResult> _callback;
        private bool _isRunning = false;
        private bool _succeeded = false;

        private float? _mainAnimationLength;
        private float? _mainAnimationRemainingTime;


        // Content
        public void Initialize(IMonsterInternal monster) => _monster = monster;

        public bool TryHit(
            out ActionResult reason,
            Action<ActionResult> callback = null,
            bool allowRestart = false,
            float? playTime = null)
        {
            if (!_monster.IsValid())
            {
                reason = new(ResultType.InvalidOperation,Ctx(
                    $"{nameof(_monster)} 컴포넌트 '{_monster?.name ?? "null"}'이(가) 유효하지 않습니다."));

                return false;
            }

            if (!allowRestart && _isRunning)
            {
                reason = new(ResultType.AlreadyDoing, Ctx(
                    $"이미 Hit 행동을 실행하고 있기 때문에 행동을 재실행할 수 없습니다."));

                return false;
            }

            _callback = callback;

            _mainAnimationLength = playTime.HasValue
                ? (playTime.Value >= 0 ? playTime.Value : null)
                : _monster.StatsInfo.InvincibleDuration;

            _mainAnimationRemainingTime = _mainAnimationLength;

            _originalMaterial = _monster.SpriteRenderer.material;
            _monster.SpriteRenderer.material = _monster.SceneAssetsLibrary.SolidColor;
            _monster.SpriteRenderer.material.color = Color.white;

            _isRunning = true;
            _succeeded = false;
            _materialRestored = false;

            reason = new(ResultType.Success);
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
            if (!_isRunning)
                return;

            if (!_mainAnimationRemainingTime.HasValue)
                return;

            if (!_monster.SpriteRenderer)
            {
                _isRunning = false;
                return;
            }


            _mainAnimationRemainingTime -= Time.deltaTime;

            if (_mainAnimationRemainingTime <= _mainAnimationLength - DamageFlashDuration)
                RestoreMaterial();

            if (_mainAnimationRemainingTime <= 0)
            {
                _succeeded = true;
                StopAction();
            }
        }

        public void StopAction()
        {
            RestoreMaterial();
            _isRunning = false;

            _callback?.Invoke(new(_succeeded
                ? ResultType.Success
                : ResultType.Interrupted));
        }

        private void RestoreMaterial()
        {
            if (_materialRestored)
                return;

            if (_monster.IsValid() && _monster.SpriteRenderer)
            {
                if (_monster.SpriteRenderer.material)
                    Destroy(_monster.SpriteRenderer.material);

                _monster.SpriteRenderer.material = _originalMaterial;
                _materialRestored = true;
            }
        }


        private void UpdateDisplayConetnt()
        {
            if (_monster == null)
                _stateDisplay = $"Invalid {nameof(_monster)} ({_monster?.name ?? "Null"})";
            else
                _stateDisplay = $"Running: {_isRunning}\nSucceed: {_succeeded}";
        }

        private string Ctx(string message)
        {
            if (_monster.IsValid())
                return _monster.FormatLogMessage(_Ctx(message));
            else
                return _Ctx(message);

            string _Ctx(string message) => $"{GetType().Name}: {message}";
        }
    }
}
