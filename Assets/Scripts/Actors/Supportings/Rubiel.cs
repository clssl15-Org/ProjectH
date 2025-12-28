using Infrastructure;
using UnityEngine;
using MonsterSystem = Actors.Monsters.Actions;

namespace Actors
{
    [RequireComponent(typeof(SpriteSizeHandler), typeof(Animator))]
    public class Rubiel : MonoBehaviour
    {
        public enum State { None, Small, Big }
        public State CurrentState { get; private set; } = State.None;
        [field: SerializeField] public bool ChangeState_T { get; set; } = false;

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
            if (ChangeState_T && Input.GetKeyDown(KeyCode.T))
            {
                if (CurrentState == State.Small)
                    ToBig();
                else
                    ToSmall();
            }
        }

        public void ToBig()
        {
            if (CurrentState == State.Big) return;
            CurrentState = State.Big;

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
            if (CurrentState == State.Small) return;
            CurrentState = State.Small;

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
            if (CurrentState == State.Big) return;
            CurrentState = State.Big;

            _player.Play(new("Big"));
            ValidateSpriteSize();
        }
        public void SetToSmall()
        {
            if (CurrentState == State.Small) return;
            CurrentState = State.Small;

            _player.Play(new("Small"));
            ValidateSpriteSize();
        }

        private void ValidateSpriteSize() => _ssh.RequestApplyScaleFactor();
    }
}
