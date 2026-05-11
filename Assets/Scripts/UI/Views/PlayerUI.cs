using System;
using System.Collections.Generic;
using System.Linq;
using BlackboxSystem;
using Infrastructure;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class PlayerUI : MonoBehaviour,
        IView,
        IInputLayerSubject
    {
        // Front
        public bool AllowInput { get; set; } = true;
        bool IInputLayerSubject.IsTrigger { get; } = true;

        public event Action Destroying;

        // Internal
        [SerializeField] private PlayerView.SkillUI _skillUI;
        [SerializeField] private Button _skillBtn;

        [SerializeField] private Button _defaultAttackBtn;
        [SerializeField] private Button _rangedAttackBtn;
        [SerializeField] private PlayerView.UltimateUI _ultimateUI;
        [SerializeField] private Button _ultimateBtn;
        [Space]
        [SerializeField] private HealthBarUI _healthBar;
        [SerializeField] private PlayerView.RelicManager _relicManager;

        private RectTransform _transform;
        private PlayerVM _player;

        private readonly HashSet<Button> _currentSelectedButtons = new();
        private readonly HashSet<Button> _selectedButtons = new();


        private readonly int[] _probTable = new int[] { 0, 10, 25, 50, 75, 100 };
        private bool _isAwaked = false;


        // Content
        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            if (_isAwaked) return;
            _isAwaked = true;

            _transform = GetComponent<RectTransform>();
        }

        public void Connect(PlayerVM player)
        {
            using var _ = BlackboxHandle.Of(this).ExertScope(player, $"Connect: {player}");
            _player = player;

            _skillUI.InitializeSkills(player.HavingSkills.ToArray());
            player.SkillAdded += _skillUI.AddSkill;
            player.SkillChanged += _skillUI.OnSkillChanged;
            player.SkillRouletteApplied += _skillUI.ShowRouletteResult;
            player.SkillRouletteCleared += _skillUI.ClearRouletteResult;

            player.CooltimeEnabled += _skillUI.EnableCooltime;
            player.CooltimeDisabled += _skillUI.DisableCooltime;

            _ultimateUI.Initialize(() => player.CurrentUltimateCooldown);
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
            RelicManager.Instance.RelicLost += OnRelicLost;
            #endregion
        }

        private void OnRelicAcquired(RelicDataSO relicSO, string _)
        {
            using var __ = BlackboxHandle.Of(this).ExertScope(_relicManager, $"Relic Acquired: {relicSO.name}");
            _relicManager.AddRelic(relicSO);
        }
        private void OnRelicLost(RelicDataSO relicSO)
        {
            using var __ = BlackboxHandle.Of(this).ExertScope(_relicManager, $"Relic Lost: {relicSO.name}");

            var relicId = relicSO.RelicNumber;
            // 같은 종류의 유물이 아직 남아 있으면 HUD 아이콘은 유지합니다.
            if (RelicManager.Instance
                && RelicManager.Instance.OwnedRelics.TryGetValue(relicId, out var ownedRelics)
                && ownedRelics.Count > 0)
            {
                return;
            }

            _relicManager.RemoveRelic(relicId);
        }


        public void Disconnect()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Disconnect");

            if (_player != null)
            {
                BlackboxHandle.Of(this).Exert(_player, "Disconnect");

                _player.SkillAdded -= _skillUI.AddSkill;
                _player.SkillChanged -= _skillUI.OnSkillChanged;
                _player.SkillRouletteApplied -= _skillUI.ShowRouletteResult;
                _player.SkillRouletteCleared -= _skillUI.ClearRouletteResult;
                _player.CooltimeEnabled -= _skillUI.EnableCooltime;
                _player.CooltimeDisabled -= _skillUI.DisableCooltime;
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
                RelicManager.Instance.RelicLost -= OnRelicLost;
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
            using var _ = BlackboxHandle.Of(this).WriteScope("Apply Random Skill Buff");
            if (!_player.TrySkillRoulette(out var appliedBonus, out var apply)) return;

            BlackboxHandle.Of(this).Write($"Bouns: {appliedBonus}");

            // _probTable은 0부터 100까지 정수
            appliedBonus = Mathf.RoundToInt((appliedBonus - 1) * 100);

            var index = _probTable.Count(prob => appliedBonus >= prob) - 1;
            index = Mathf.Clamp(index, 0, _probTable.Length - 1);

            BlackboxHandle.Of(this).Write($"Index: {index}");
            _skillUI.EnableRoulette(index, () =>
            {
                BlackboxHandle.Of(this).Exert(_player, "Apply Damage");
                apply();
            });
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
    }
}
