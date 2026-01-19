using System;
using System.Linq;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public class TwinBossManager : MonoBehaviour
    {
        // Front
        public event Action Cleared;

        // Internal
        [SerializeField] private GameObject[] _bosses;
        [SerializeField] private float _reviveTime = 10f;
        [SerializeField] private float _bonusTime = 5f;
        [SerializeField] private KeyCode _forceClearKey = KeyCode.None;

        private bool _isPending;
        private bool _isCleared;

        private int _pendingCount;
        private ITwinBoss[] _twinBosses;
        private float _reviveTimer;


        // Content
        private void Awake()
        {
            _twinBosses = _bosses
                .Select(go => go.GetComponent<ITwinBoss>())
                .ToArray();
        }

        public void Commence()
        {
            _isPending = false;
            _isCleared = false;
            _pendingCount = 0;

            foreach (var boss in _twinBosses)
                boss.Commence();
        }

        private void Update()
        {
            if (Input.GetKeyDown(_forceClearKey))
            {
                TwinBossClear();
                return;
            }

            if (_isCleared)
                return;

            if (_twinBosses.All(b => b.IsExhausted))
            {
                // 공략 성공
                TwinBossClear();
                return;
            }

            if (_twinBosses.All(b => !b.IsExhausted))
            {
                // 둘 다 살아있음
                _isPending = false;
                return;
            }

            if (!_isPending)
            {
                // 이제 막 1명만 남은 상태가 됨
                _isPending = true;
                _reviveTimer = _reviveTime;

                // 추가시간 적용
                _reviveTimer += _pendingCount * _bonusTime;
                _pendingCount++;

                return;
            }

            // 타이머 가동
            _reviveTimer -= Time.deltaTime;
            if (_reviveTimer <= 0f)
            {
                _twinBosses
                    .First(b => b.IsExhausted)
                    .Revive(0.5f); // 기존 체력의 x배로 부활

                _isPending = false; // 이제 2명이 살아있는 상태
            }
        }

        private void TwinBossClear()
        {
            _isCleared = true;

            foreach (var boss in _twinBosses)
                boss.SetToDead();

            Cleared?.Invoke();
        }
    }
}
