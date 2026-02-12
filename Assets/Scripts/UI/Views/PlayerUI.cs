using System;
using System.Collections.Generic;
using System.Linq;
using BlackboxSystem;
using Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Video;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class PlayerUI : MonoBehaviour,
        IView,
        IEnablable,
        IInputControllable
    {
        // Front
        public bool AllowInput { get; set; } = true;
        public event Action Destroying;

        // Internal
        [SerializeField] private PlayerView.SkillManager _skillManager;
        [SerializeField] private Button _skillBtn;
        [SerializeField] private Animation _skillRouletteBackground;
        [SerializeField] private VideoPlayer _skillRoulette;
        [SerializeField] private Button _defaultAttackBtn;
        [SerializeField] private Button _rangedAttackBtn;
        [SerializeField] private Button _ultimateBtn;
        [Space]
        [SerializeField] private HealthBarUI _healthBar;
        [SerializeField] private PlayerView.RelicManager _relicManager;

        [Header("Resources")]
        [SerializeField, Min(0)] private float _skillRouletteVideoPlaytime = 3.5f;
        [SerializeField] private VideoClip[] _skillRouletteVideos;

        #region Interfaces
        Action IEnablable.OnEnabling => 
            () => _skillRouletteBackground.gameObject.SetActive(true);
        Action IEnablable.OnEnabled => null;
        Action IEnablable.OnDisabling => null;
        Action IEnablable.OnDisabled =>
            () => _skillRouletteBackground.gameObject.SetActive(false);
        #endregion

        private RectTransform _transform;
        private PlayerVM _player;

        private readonly HashSet<Button> _currentSelectedButtons = new();
        private readonly HashSet<Button> _selectedButtons = new();

        private EnableWithAnimation _skillRouletteEnabler;
        private IDisposable _skillRouletteDeactivateTimer;

        private bool _isSkillRulettelocked = false;
        private bool _isAwaked = false;


        // Content
        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            if (_isAwaked) return;
            _isAwaked = true;

            _transform = GetComponent<RectTransform>();

            _skillRouletteEnabler = new EnableWithAnimation(_skillRouletteBackground, false)
                .InitializeWithIEnablable(this, false);
            _skillRouletteEnabler.SetToDisabled();

            _skillRoulette.clip = null;
        }

        public void Connect(PlayerVM player)
        {
            using var _ = BlackboxHandle.Of(this).ExertScope(player, $"Connect: {player}");
            _player = player;

            _skillManager.Initialize(player.HavingSkills.ToArray());
            player.SkillAdded += _skillManager.AddSkill;
            player.SkillChanged += _skillManager.OnSkillChanged;

            _skillBtn.onClick.AddListener(ApplyRandomSkillBuff);

            // TODO: Player Input 배선 작업
            //_defaultAttackBtn.onClick.AddListener(_player.DefaultAttack);
            //_rangedAttackBtn.onClick.AddListener(_player.RangedAttack);
            //_ultimateBtn.onClick.AddListener(null);
            
            _healthBar.Connect(_player);


            #region RelicManager 연결
            if (!RelicManager.Instance)
                throw new InvalidOperationException(BlackboxHandle.Of(this).WriteError(
                    $"[{nameof(PlayerUI)}] {nameof(RelicManager.Instance)}이(가) 유효하지 않습니다."));
            BlackboxHandle.Of(this).Exert(RelicManager.Instance, "Connect");

            foreach (var id in RelicManager.Instance.OwnedRelics.Keys)
            {
                if (!RelicManager.Instance.TryGetRelicData(id, out var relicData))
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"[{nameof(PlayerUI)}] Relic ID '{id}'에 해당하는 {nameof(RelicDataSO)}을(를) 찾을 수 없습니다."),
                        this);
                    continue;
                }

                _relicManager.AddRelic(relicData);
            }

            RelicManager.Instance.RelicAcquired += OnRelicAcquired;
            #endregion
        }

        private void OnRelicAcquired(RelicDataSO relicSO, string _)
        {
            using var __ = BlackboxHandle.Of(this).ExertScope(_relicManager, $"Relic Acquired: {relicSO.name}");
            _relicManager.AddRelic(relicSO);
        }

        public void Disconnect()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Disconnect");

            if (_player != null)
            {
                BlackboxHandle.Of(this).Exert(_player, "Disconnect");

                _player.SkillAdded -= _skillManager.AddSkill;
                _player.SkillChanged -= _skillManager.OnSkillChanged;
                _player = null;
            }

            _skillBtn.onClick.RemoveAllListeners();
            _defaultAttackBtn.onClick.RemoveAllListeners();
            _rangedAttackBtn.onClick.RemoveAllListeners();
            _ultimateBtn.onClick.RemoveAllListeners();

            _healthBar.Disconnect();

            if (RelicManager.Instance)
            {
                BlackboxHandle.Of(this).Exert(RelicManager.Instance, "Disconnect");
                RelicManager.Instance.RelicAcquired -= OnRelicAcquired;
            }
        }

        // 여기서 UI 이벤트 처리
        private void Update()
        {
            if (!AllowInput)
                return;

            _currentSelectedButtons.Clear();

            if (Input.GetKeyDown(KeyCode.E))
                SelectNextSkill();

            if (Mathf.Abs(Input.GetAxis("Mouse ScrollWheel")) > 0.01f)
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
            if (_player == null)
                Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                    "[PlayerUI] Player가 null이기 때문에 SelectNextSkill 메서드를 실행할 수 없습니다."));

            _player.ChangeSkill();
        }

        private void ApplyRandomSkillBuff()
        {
            if (!_player.CanApplySkillBuff)
                return;

            if (_isSkillRulettelocked) return;
            _isSkillRulettelocked = true;

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
                        _isSkillRulettelocked = false;

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
            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");

            Disconnect();
            Destroying?.Invoke();

            if (this && gameObject)
                Destroy(gameObject);
        }

        void IEnablable.Enable() =>
            throw new NotImplementedException();
        void IEnablable.Disable() =>
            throw new NotImplementedException();
        void IEnablable.SetToEnabled() =>
            throw new NotImplementedException();
        void IEnablable.SetToDisabled() =>
            throw new NotImplementedException();
    }
}
