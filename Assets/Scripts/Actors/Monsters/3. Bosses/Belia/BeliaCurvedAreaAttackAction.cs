using Actors.Monsters.Actions;
using Infrastructure;
using Unity.VisualScripting;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Belia
    {
        private class BeliaCurvedAreaAttackAction : MonsterActionComponent
        {
            public float EffectLength { get; set; }

            private GameObject _curveEffectPrefab;
            private Vector2 _curveEffectWorldPosition;

            private GameObject _curveEffectInstance;
            private Material _effectMat;
            private float _effectRemainingTime;

            private MonsterAnimationPlayer AnimationPlayer => MonsterAction.Owner.AnimationPlayer;

            public BeliaCurvedAreaAttackAction(
                GameObject curveEffectPrefab,
                Vector2 curveEffectWorldPosition,
                float effectLength = 1)
            {
                _curveEffectPrefab = curveEffectPrefab;
                _curveEffectWorldPosition = curveEffectWorldPosition;
                EffectLength = effectLength;
            }

            protected override void OnEnter(float _, object __)
            {
                _curveEffectInstance = Instantiate(_curveEffectPrefab);
                _curveEffectInstance.transform.localScale = new Vector3(
                    Owner.Direction == Direction.Right ? 1 : -1, 1, 1);
                _curveEffectInstance.transform.position = _curveEffectWorldPosition;
                _curveEffectInstance.SetActive(true);

                _effectMat = _curveEffectInstance.GetComponent<SpriteRenderer>().material;
                _effectRemainingTime = EffectLength;
            }

            protected override void OnUpdate(float _)
            {
                _effectRemainingTime -= Time.deltaTime;
                _effectMat.color = _effectMat.color.WithAlpha(_effectRemainingTime / EffectLength);

                if (_effectRemainingTime <= 0)
                    Interrupt(InterruptType.Completed);
            }

            protected override void OnInterrupt(InterruptType _)
            {
                Destroy(_curveEffectInstance);
            }
        }
    }
}
