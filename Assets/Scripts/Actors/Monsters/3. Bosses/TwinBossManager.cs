using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Actors.Monsters.Stage3Bosses
{
    public class TwinBossManager : MonoBehaviour
    {
        // Internal
        [SerializeField] private GameObject[] _bosses;
        [SerializeField] private float _reviveTime = 10f;
        [SerializeField] private float _bonusTime = 5f;

        private bool _isPending = false;
        private ITwinBoss[] _twinBosses;
        private float _reviveTimer;


        // Content
        private void Awake()
        {
            _twinBosses = _bosses
                .Select(go => go.GetComponent<ITwinBoss>())
                .ToArray();
        }

        public void DoAwake()
        {
            foreach (var boss in _twinBosses)
                boss.DoAwake();
        }

        private void Update()
        {
            if (_twinBosses.All(b => b.IsExhausted))
            {
                print("공략 성공");
                gameObject.SetActive(false);
                return;
            }

            if (_twinBosses.All(b => !b.IsExhausted))
            {
                _isPending = false;
                return;
            }

            if (!_isPending)
            {
                _isPending = true;
                _reviveTimer = _reviveTime;

                return;
            }

            _reviveTimer -= Time.deltaTime;
            if (_reviveTimer <= 0f)
            {
                _twinBosses
                    .First(b => b.IsExhausted)
                    .Revive(0.5f);

                _isPending = false;
            }
        }
    }
}
