using System;
using System.Collections.Generic;
using System.Linq;
using Actors.Monsters.Bosses;
using Infrastructure;
using UI.PlayerView;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;
using MonsterSystem = Actors.Monsters.Actions;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class PlayerUI : MonoBehaviour, IView, IEnablable
    {
        // Front
        public event Action Destroyed;

        // Internal
        [SerializeField] private Button _skillBtn;
        [SerializeField] private Animation _skillRouletteBackground;
        [SerializeField] private VideoPlayer _skillRoulette;
        [SerializeField] private Button _defaultAttackBtn;
        [SerializeField] private Button _rangedAttackBtn;
        [SerializeField] private Button _ultimateBtn;
        [Space]
        [SerializeField] private HealthBar _healthBar;
        [SerializeField] private UI.PlayerView.RelicManager _relicManager;

        [Header("Resources")]
        [SerializeField, Min(0)] private float _skillRouletteVideoPlaytime = 3.5f;
        [SerializeField] private VideoClip[] _skillRouletteVideos;

        #region Interfaces
        Action IEnablable.Enabling =>
            () => _skillRouletteBackground.gameObject.SetActive(true);
        Action IEnablable.Enabled =>
            null;
        Action IEnablable.Disabling =>
            null;
        Action IEnablable.Disabled =>
            () => _skillRouletteBackground.gameObject.SetActive(false);
        #endregion

        private RectTransform _transform;
        private PlayerVM _player;

        private readonly HashSet<Button> _currentSelectedButtons = new();
        private readonly HashSet<Button> _selectedButtons = new();

        private EnableWithAnimation _skillRouletteEnabler;
        private IDisposable _skillRouletteDeactivateTimer;

        private bool _skillRulettelocked = false;
        private MonsterSystem.MonsterAnimationPlayer _skillAnimPlayer;
        private int _currentSelectedSkillIndex = 0;

        private bool _awaked = false;


        // Content
        private void Awake()
        {
            if (_awaked) return;
            _awaked = true;

            _transform = GetComponent<RectTransform>();

            Animator skillAnimator = null;
            if (!(_skillBtn?.TryGetComponent<Animator>(out skillAnimator) ?? false))
                throw new InvalidOperationException(
                    $"[{nameof(PlayerUI)}] '{nameof(_skillBtn)}' 컴포넌트는 {nameof(Animator)}을(를) 가지고 있어야 합니다.");

            _skillRouletteEnabler = new EnableWithAnimation(_skillRouletteBackground, false)
                .InitializeWithIEnablable(this, false);
            _skillRouletteEnabler.SetToDisabled();

            _skillRoulette.clip = null;
            _skillAnimPlayer = new(skillAnimator);
        }

        public void Connect(PlayerVM player)
        {
            _player = player;

            _skillBtn.onClick.AddListener(ApplyRandomSkillBuff);
            _defaultAttackBtn.onClick.AddListener(_player.DefaultAttack);
            _rangedAttackBtn.onClick.AddListener(_player.RangedAttack);
            //_ultimateBtn.onClick.AddListener(null);
            

            _healthBar.Connect(_player);

            foreach (var id in _player.Relics)
                _relicManager.AddRelic(id);

            _player.RelicAcquired += _relicManager.AddRelic;
            _player.RelicAbandoned += _relicManager.RemoveRelic;
        }

        public void Disconnect()
        {
            if (_player == null)
                return;

            _skillBtn.onClick.RemoveAllListeners();
            _defaultAttackBtn.onClick.RemoveAllListeners();
            _rangedAttackBtn.onClick.RemoveAllListeners();
            _ultimateBtn.onClick.RemoveAllListeners();


            _healthBar.Disconnect();

            _player.RelicAcquired -= _relicManager.AddRelic;
            _player.RelicAbandoned -= _relicManager.RemoveRelic;

            _player = null;
        }

        // 여기서 UI 이벤트 처리
        private void Update()
        {
            _currentSelectedButtons.Clear();

            if (Input.GetKeyDown(KeyCode.E))
                SelectNextSkill();

            if (Input.GetMouseButtonDown(2)
                || Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.0001f)
                _currentSelectedButtons.Add(_skillBtn);

            if (Input.GetMouseButton(0))
                _currentSelectedButtons.Add(_defaultAttackBtn);

            if (Input.GetMouseButton(1))
                _currentSelectedButtons.Add(_rangedAttackBtn);


            var ped = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left
            };

            foreach (var btn in _selectedButtons)
            {
                if (_currentSelectedButtons.Contains(btn))
                    continue;

                ExecuteEvents.Execute(btn.gameObject, ped, ExecuteEvents.pointerUpHandler);
                EventSystem.current.SetSelectedGameObject(null); // 버튼 뗐을 때 하이라이트 잔상 없애기
            }

            foreach (var btn in _currentSelectedButtons)
            {
                if (_selectedButtons.Contains(btn))
                    continue;

                btn.Select();
                ExecuteEvents.Execute(btn.gameObject, ped, ExecuteEvents.pointerClickHandler);
                ExecuteEvents.Execute(btn.gameObject, ped, ExecuteEvents.pointerDownHandler);
            }

            _selectedButtons.Clear();
            _selectedButtons.UnionWith(_currentSelectedButtons);
        }

        private void SelectNextSkill()
        {
            var animName = _currentSelectedSkillIndex switch
            {
                0 => "1 to 2",
                1 => "2 to 3",
                2 => "3 to 1",
                var i => throw new ArgumentOutOfRangeException(
                    nameof(_currentSelectedSkillIndex), i , "인자는 0 이상 2 이하여야 합니다.")
            };

            _currentSelectedSkillIndex = (_currentSelectedSkillIndex + 1) % 3;
            _skillAnimPlayer.Play(new(animName));

            _player.ChangeSkill(_currentSelectedSkillIndex);
        }

        private void ApplyRandomSkillBuff()
        {
            if (_skillRulettelocked) return;
            _skillRulettelocked = true;

            // TODO: 이 부분 PlayerVM으로 옮기기
            var table = new (float weight, int index, float factor)[]
            {
                (22, 0, 0),
                (30, 1, 10),
                (25, 2, 25),
                (15, 3, 50),
                (6,  4, 75),
                (2,  5, 100),
            };

            float selector = UnityEngine.Random.Range(0f, table.Sum(t => t.weight));
            float criteria = 0f;

            int index = table[^1].index;
            float factor = table[^1].factor;

            foreach (var item in table)
            {
                criteria += item.weight;
                if (selector < criteria)
                {
                    (index, factor) = (item.index, item.factor);
                    break;
                }
            }

            _skillRouletteDeactivateTimer?.Dispose();
            _skillRouletteDeactivateTimer = new Timer(
                _skillRouletteVideoPlaytime,
                succeeded =>
                {
                    if (succeeded)
                    {
                        _skillRouletteEnabler.Disable();
                        _skillRoulette.clip = null;
                        _skillRulettelocked = false;

                        _player.ApplyRandomSkillBuff(factor);
                    }
                });

            _skillRouletteEnabler.Enable();
            _skillRoulette.clip = _skillRouletteVideos[index];
            _skillRoulette.Play();
        }



        public void SetParent(RectTransform parent)
        {
            Awake();
            _transform.SetParent(parent);
        }

        public void Destroy()
        {
            Disconnect();
            Destroyed?.Invoke();

            if (this && gameObject)
                Destroy(gameObject);
        }

        #region Interfaces
        void IEnablable.Enable() =>
            throw new InvalidOperationException();
        void IEnablable.Disable() =>
            throw new InvalidOperationException();
        void IEnablable.SetToEnabled() =>
            throw new InvalidOperationException();
        void IEnablable.SetToDisabled() =>
            throw new InvalidOperationException();
        #endregion
    }
}
