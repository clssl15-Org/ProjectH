using Infrastructure;
using UnityEngine;
using MonsterSystem = Actors.Monsters.Actions;

namespace Actors
{
    [RequireComponent(typeof(SpriteSizeHandler), typeof(Animator))]
    public class Rubiel : MonoBehaviour
    {
        public enum Shape { None, Small, Big }
        public Shape CurrentShape { get; private set; } = Shape.None;
        [SerializeField] private bool _changeShape_T = false;

        private SpriteSizeHandler _ssh;
        private MonsterSystem.MonsterAnimationPlayer _player;


        private void Awake()
        {
            _ssh = GetComponent<SpriteSizeHandler>();
            _player = new(GetComponent<Animator>());
        }

        private void Start() => SetToSmall();

        private void Update()
        {
            if (_changeShape_T && Input.GetKeyDown(KeyCode.T))
            {
                if (CurrentShape == Shape.Small)
                    ToBig();
                else
                    ToSmall();
            }
        }

        public void ToBig()
        {
            if (CurrentShape == Shape.Big) return;
            CurrentShape = Shape.Big;

            _player.Play(new("SmallToBig", Callback: succeeded =>
            {
                if (succeeded)
                {
                    _player.Play(new("Big"));
                    ValidateSpriteSize();
                }
            }));
            ValidateSpriteSize();
        }
        public void ToSmall()
        {
            if (CurrentShape == Shape.Small) return;
            CurrentShape = Shape.Small;

            _player.Play(new("BigToSmall", Callback: succeeded =>
            {
                if (succeeded)
                {
                    _player.Play(new("Small"));
                    ValidateSpriteSize();
                }
            }));
            ValidateSpriteSize();
        }

        public void SetToBig()
        {
            if (CurrentShape == Shape.Big) return;
            CurrentShape = Shape.Big;

            _player.Play(new("Big"));
            ValidateSpriteSize();
        }
        public void SetToSmall()
        {
            if (CurrentShape == Shape.Small) return;
            CurrentShape = Shape.Small;

            _player.Play(new("Small"));
            ValidateSpriteSize();
        }

        private void ValidateSpriteSize() => _ssh.RequestApplyScaleFactor();
    }
}
