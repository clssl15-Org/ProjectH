using System;
using UnityEngine;
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


        // Content
        private void Awake() =>
            _transform = GetComponent<RectTransform>();

        public void Connect(PlayerVM player)
        {
            _player = player;

            // 여기서 연결 처리
            _healthBar.Connect(_player);
        }

        public void Disconnect()
        {
            if (_player == null)
                return;

            // 여기서 연결 해제
            _healthBar.Disconnect();

            _player = null;
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
