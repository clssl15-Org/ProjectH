using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(RectTransform))]
    public class PlayerUI : MonoBehaviour, IView
    {
        // Front
        public event Action Destroyed;

        // Internal
        [SerializeField] private Button _skillBtn;
        [SerializeField] private Button _defaultAttackBtn;
        [SerializeField] private Button _rangedAttackBtn;
        [SerializeField] private Button _ultimateBtn;
        [Space]
        [SerializeField] private HealthBar _healthBar;

        private RectTransform _transform;
        private PlayerVM _player;

        private readonly HashSet<Button> _currentSelectedButtons = new();
        private readonly HashSet<Button> _selectedButtons = new();


        // Content
        private void Awake() =>
            _transform = GetComponent<RectTransform>();

        public void Connect(PlayerVM player)
        {
            _player = player;

            // 여기서 연결 처리
            _healthBar.Connect(_player);

            _skillBtn.onClick.AddListener(() => { });
            _defaultAttackBtn.onClick.AddListener(() => _player.DefaultAttack());
            _rangedAttackBtn.onClick.AddListener(() => { });
            _ultimateBtn.onClick.AddListener(() => { });
        }

        public void Disconnect()
        {
            if (_player == null)
                return;

            // 여기서 연결 해제
            _healthBar.Disconnect();

            _player = null;
        }

        // 여기서 버튼 이벤트 처리
        private void Update()
        {
            _currentSelectedButtons.Clear();

            if (Input.GetKey(KeyCode.E) || Input.GetMouseButton(2))
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

                // 시각적 효과 해제 (Pressed -> Normal/Highlighted)
                ExecuteEvents.Execute(btn.gameObject, ped, ExecuteEvents.pointerUpHandler);
                // 기능 실행 (Click)
                ExecuteEvents.Execute(btn.gameObject, ped, ExecuteEvents.pointerClickHandler);
                // 버튼 뗐을 때 하이라이트 잔상 없애기
                EventSystem.current.SetSelectedGameObject(null); 
            }

            foreach (var btn in _currentSelectedButtons)
            {
                if (_selectedButtons.Contains(btn))
                    continue;

                btn.Select();
                ExecuteEvents.Execute(btn.gameObject, ped, ExecuteEvents.pointerDownHandler);
            }

            _selectedButtons.Clear();
            _selectedButtons.UnionWith(_currentSelectedButtons);
        }

        public void SetParent(RectTransform parent) =>
            _transform.SetParent(parent);

        public void Destroy()
        {
            Disconnect();
            Destroyed?.Invoke();

            if (this && gameObject)
                Destroy(gameObject);
        }
    }
}
