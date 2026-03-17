using System;
using UnityEngine;

namespace Actors.Monsters.Actions
{
    internal class AttackWithWeapon : MonsterActionComponent
    {
        // Internal
        private IWeapon _weaponPrefab;
        private float _startTime;
        private float _duration;

        private Payload _payload;
        private Func<bool> _checkCondition;

        private IWeapon _weapon;
        private Action<IWeapon> _onInstantiated;
        private float _elapsedTime;
        private int _phase;


        // Content
        public AttackWithWeapon(
            IWeapon weaponPrefab,
            float startTime = 0,
            float duration = -1,
            Action<IWeapon> onInstantiate = null)
        {
            _weaponPrefab = weaponPrefab;
            _startTime = startTime;
            _duration = duration < 0 ? float.MaxValue : duration;
            _onInstantiated = onInstantiate;
        }

        public record Payload
        (
            Func<IWeapon, Func<bool>> GetCheckCondition = null,
            MonsterConditionData MonsterConditionData = null
        );
        protected override void OnEnter(object input)
        {
            if (input != null)
            {
                if (input is not Payload payload)
                    throw new ArgumentException(
                        Ctx($"{nameof(input)}은(는) null이거나 {nameof(Payload)} 형식이어야 하지만 '{input.GetType().Name}' 형식이 입력되었습니다."),
                        nameof(input));

                _payload = payload;
            }
            else
                _payload = null;

            if (_weaponPrefab == null)
            {
                Debug.LogWarning(
                    Ctx($"{nameof(_weaponPrefab)}이(가) 유효하지 않으므로 컴포넌트가 비활성화되었습니다."),
                    Owner.gameObject);

                Interrupt(InterruptType.Completed);
                return;
            }

            _elapsedTime = 0;
            _phase = 0;

            if (_startTime <= 0)
            {
                _phase = 1;
                SetWeapon();
            }
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (_phase >= 2)
                return;

            if (!(_checkCondition?.Invoke() ?? true))
            {
                _phase = 2;
                UnsetWeapon();
                return;
            }

            _elapsedTime += deltaTime;

            if (_phase == 0 && _elapsedTime >= _startTime)
            {
                _phase = 1;
                SetWeapon();
            }

            if (_phase == 1 && _elapsedTime >= _startTime + _duration)
            {
                _phase = 2;
                UnsetWeapon();
            }
        }

        private void SetWeapon()
        {
            _weapon = UnityEngine.Object
                .Instantiate(_weaponPrefab.gameObject)
                .GetComponent<IWeapon>();
            _weapon.transform.SetParent(Owner.transform);

            _weapon.transform.SetPositionAndRotation(_weaponPrefab.transform.position, _weapon.transform.rotation);
            _weapon.transform.localScale = _weaponPrefab.transform.localScale;

            _onInstantiated?.Invoke(_weapon);
            _weapon.gameObject.SetActive(true);

            if (_payload != null)
            {
                if (_payload.MonsterConditionData != null)
                {
                    var attackData = (MonsterAttackData)_payload.MonsterConditionData.Payload;

                    attackData.OnExecuting();
                    _weapon.SetHitPlayerCallback(attackData.OnHit);
                }

                if (_payload.GetCheckCondition != null)
                    _checkCondition = _payload.GetCheckCondition(_weapon);
            }
        }

        private void UnsetWeapon()
        {
            if (_weapon != null && _weapon.gameObject)
                UnityEngine.Object.Destroy(_weapon.gameObject);

            _weapon = null;
        }
        
        protected override void OnInterrupt(InterruptType _)
        {
            _checkCondition = null;
            UnsetWeapon();
        }

        private string Ctx(string message) => $"[{nameof(AttackWithWeapon)}] {message}";
    }
}
