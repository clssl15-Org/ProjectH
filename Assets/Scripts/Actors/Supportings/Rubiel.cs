using System;
using BlackboxSystem;
using Infrastructure;
using Unity.VisualScripting;
using UnityEngine;
using MonsterSystem = Actors.Monsters.Actions;

namespace Actors
{
    [RequireComponent(typeof(SpriteRenderer), typeof(SpriteSizeHandler), typeof(Animator))]
    [RequireComponent(typeof(TargetFollower))]
    public class Rubiel : MonoBehaviour
    {
        public enum Shape { None, Small, Big }
        public Shape CurrentShape { get; private set; } = Shape.None;

        public enum Visibility { None, Visible, Invisible }
        public Visibility CurrentVisibility { get; private set; } = Visibility.None;
        [field: SerializeField] public float VisibleSpeed { get; set; } = 1f;

        public bool FollowerEnabled
        {
            get => _targetFollower.enabled;
            set => _targetFollower.enabled = value;
        }

        public Vector2 AnchorPos => _targetFollower.AnchorPos;

        public bool IsTotallyVisible => _spriteRenderer.material.color.a >= 1f;
        public bool IsTotallyInvisible => _spriteRenderer.material.color.a <= 0f;

        [SerializeField] private Transform _player;
        [SerializeField] private KeyCode _changeShapeKey = KeyCode.None;
        [SerializeField] private KeyCode _changeVisibilityKey = KeyCode.None;

        private SpriteRenderer _spriteRenderer;
        private SpriteSizeHandler _ssh;
        private TargetFollower _targetFollower;
        private MonsterSystem.MonsterAnimationPlayer _animPlayer;
        private IDisposable _visibilityChanger;


        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            if (!_player)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteMessage(
                    $"[Rubiel] {nameof(_player)}이(가) 유효하지 않습니다."));

            _spriteRenderer = GetComponent<SpriteRenderer>();   
            _ssh = GetComponent<SpriteSizeHandler>();
            _animPlayer = new(GetComponent<Animator>());

            _targetFollower = GetComponent<TargetFollower>();
            _targetFollower.Initialize(_player);

            _spriteRenderer.material = Instantiate(_spriteRenderer.material);
        }

        private void Start() => SetToSmall();

        private void Update()
        {
            if (Input.GetKeyDown(_changeShapeKey.Resolve()))
                ChangeShape();
            if (Input.GetKeyDown(_changeVisibilityKey.Resolve()))
                ChangeVisibility();
        }

        public void Teleport(Vector2 position, bool lookRight)
        {
            _targetFollower.Teleport(position, lookRight);
        }

        public void ChangeShape()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Change Shape");

            if (CurrentShape == Shape.Small)
                ToBig();
            else
                ToSmall();
        }
        public void ChangeVisibility()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Change Visibility");

            if (CurrentVisibility == Visibility.Invisible)
                ToVisible();
            else
                ToInvisible();
        }

        public void ToBig(Action callback = null)
        {
            if (CurrentShape == Shape.Big) return;
            CurrentShape = Shape.Big;

            _targetFollower.IsEnabled = false;

            _animPlayer.Play(new("SmallToBig", Callback: _ =>
            {
                _animPlayer.Play(new("Big"));
                ValidateSpriteSize();
                callback?.Invoke();
            }));
            ValidateSpriteSize();
        }
        public void ToSmall(Action callback = null)
        {
            if (CurrentShape == Shape.Small)
            {
                callback?.Invoke();
                return;
            }

            CurrentShape = Shape.Small;
            _targetFollower.IsEnabled = true;

            _animPlayer.Play(new("BigToSmall", Callback: _ =>
            {
                _animPlayer.Play(new("Small"));
                ValidateSpriteSize();
                callback?.Invoke();
            }));

            ValidateSpriteSize();
        }

        public void SetToBig()
        {
            if (CurrentShape == Shape.Big) return;
            CurrentShape = Shape.Big;

            _targetFollower.IsEnabled = false;

            _animPlayer.Play(new("Big"));
            ValidateSpriteSize();
        }
        public void SetToSmall()
        {
            if (CurrentShape == Shape.Small) return;
            CurrentShape = Shape.Small;

            _targetFollower.IsEnabled = true;

            _animPlayer.Play(new("Small"));
            ValidateSpriteSize();
        }


        public void ToVisible(Action callback = null)
        {
            _visibilityChanger?.Dispose();

            IDisposable vc = null;
            vc = _visibilityChanger = Loco.Subscribe(() =>
            {
                if (vc != _visibilityChanger || _spriteRenderer.material.color.a >= 1)
                {
                    _visibilityChanger.Dispose();
                    callback?.Invoke();
                    return;
                }

                var c = new Color(
                    _spriteRenderer.material.color.r,
                    _spriteRenderer.material.color.g,
                    _spriteRenderer.material.color.b,
                    _spriteRenderer.material.color.a + VisibleSpeed * Time.deltaTime);
                _spriteRenderer.material.color = c;
            });
        }
        public void ToInvisible(Action callback = null)
        {
            _visibilityChanger?.Dispose();

            IDisposable vc = null;
            vc = _visibilityChanger = Loco.Subscribe(() =>
            {
                if (vc != _visibilityChanger || _spriteRenderer.material.color.a <= 0)
                {
                    _visibilityChanger.Dispose();
                    callback?.Invoke();
                    return;
                }

                var c = new Color(
                    _spriteRenderer.material.color.r,
                    _spriteRenderer.material.color.g,
                    _spriteRenderer.material.color.b,
                    _spriteRenderer.material.color.a - VisibleSpeed * Time.deltaTime);
                _spriteRenderer.material.color = c;
            });
        }

        public void SetToVisible()
        {
            var c = new Color(
                _spriteRenderer.material.color.r,
                _spriteRenderer.material.color.g,
                _spriteRenderer.material.color.b,
                1);

            _spriteRenderer.material.color = c;
        }
        public void SetToInvisible()
        {
            var c = new Color(
                _spriteRenderer.material.color.r,
                _spriteRenderer.material.color.g,
                _spriteRenderer.material.color.b,
                0f);
            _spriteRenderer.material.color = c;
        }

        private void ValidateSpriteSize() => _ssh.RequestApplyScaleFactor();
        private void OnDestroy()
        {
            if (_visibilityChanger != null)
            {
                _visibilityChanger?.Dispose();
                _visibilityChanger = null;
            }
        }
    }
}
