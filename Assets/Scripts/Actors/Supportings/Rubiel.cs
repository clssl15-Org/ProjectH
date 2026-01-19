using System;
using BlackboxSystem;
using Infrastructure;
using UnityEngine;
using MonsterSystem = Actors.Monsters.Actions;

namespace Actors
{
    [RequireComponent(typeof(SpriteSizeHandler), typeof(Animator))]
    [RequireComponent(typeof(TargetFollower))]
    public class Rubiel : MonoBehaviour
    {
        public enum Shape { None, Small, Big }
        public Shape CurrentShape { get; private set; } = Shape.None;

        [SerializeField] private Transform _player;
        [SerializeField] private KeyCode _changeShapeKey = KeyCode.None;

        private SpriteSizeHandler _ssh;
        private TargetFollower _targetFollower;
        private MonsterSystem.MonsterAnimationPlayer _animPlayer;


        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            if (!_player)
                throw new InvalidOperationException(BlackboxHandle.Of(this).Write(
                    $"[Rubiel] {nameof(_player)}이(가) 유효하지 않습니다."));

            _ssh = GetComponent<SpriteSizeHandler>();
            _animPlayer = new(GetComponent<Animator>());

            _targetFollower = GetComponent<TargetFollower>();
            _targetFollower.Initialize(_player);
        }

        private void Start() => SetToSmall();

        private void Update()
        {
            if (Input.GetKeyDown(_changeShapeKey))
                ChangeShape();
        }

        public void ChangeShape()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Change Shape");

            if (CurrentShape == Shape.Small)
                ToBig();
            else
                ToSmall();
        }

        public void ToBig()
        {
            if (CurrentShape == Shape.Big) return;
            CurrentShape = Shape.Big;

            _targetFollower.IsEnabled = false;

            _animPlayer.Play(new("SmallToBig", Callback: _ =>
            {
                _animPlayer.Play(new("Big"));
                ValidateSpriteSize();
            }));
            ValidateSpriteSize();
        }
        public void ToSmall()
        {
            if (CurrentShape == Shape.Small) return;
            CurrentShape = Shape.Small;

            _targetFollower.IsEnabled = true;

            _animPlayer.Play(new("BigToSmall", Callback: _ =>
            {
                _animPlayer.Play(new("Small"));
                ValidateSpriteSize();
            }));
            ValidateSpriteSize();
        }

        public void SetToBig()
        {
            if (CurrentShape == Shape.Big) return;
            CurrentShape = Shape.Big;

            _animPlayer.Play(new("Big"));
            ValidateSpriteSize();
        }
        public void SetToSmall()
        {
            if (CurrentShape == Shape.Small) return;
            CurrentShape = Shape.Small;

            _animPlayer.Play(new("Small"));
            ValidateSpriteSize();
        }

        private void ValidateSpriteSize() => _ssh.RequestApplyScaleFactor();
    }
}
