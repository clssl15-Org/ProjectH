using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using BlackThunder.BlackboxSystem;
using UnityEngine;

namespace Game.Stage
{
    public class MonsterManager : MonoBehaviour
    {
        // Internal
        private readonly HashSet<IMonster> _monsters = new();
        private bool _isDestroyed = false;
        private BlackboxHandle _blackbox;


        // Content
        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct("몬스터 매니저 초기화를 시작합니다.", out _blackbox);
        }

        public bool Register(IMonster monster)
        {
            using var _ = _blackbox.Scope("몬스터를 등록합니다.").With(monster);

            if (!monster.IsValid())
                throw new ArgumentException(Ctx("��ȿ���� ���� monster ���ڰ� �ԷµǾ����ϴ�."),
                    nameof(monster));

            if (_monsters.Contains(monster))
                return false;

            _monsters.Add(monster);
            monster.Destroyed += () => _monsters.Remove(monster);

            return true;
        }

        private void OnDestroy() => Destroy();
        internal void Destroy()
        {
            using var _ = _blackbox.Scope("등록된 몬스터를 정리합니다.");

            if (_isDestroyed) return;
            _isDestroyed = true;

            _monsters.ToList().ForEach(m =>
            {
                m.Destroy();
            });
            _monsters.Clear();
        }

        private string Ctx(string message) => $"[{nameof(MonsterManager)}] {message}";
    }
}
