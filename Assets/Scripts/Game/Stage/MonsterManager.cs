using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using BlackboxSystem;
using UnityEngine;

namespace Game.Stage
{
    public class MonsterManager : MonoBehaviour
    {
        // Internal
        private readonly HashSet<IMonster> _monsters = new();
        private bool _isDestroyed = false;


        // Content
        public bool Register(IMonster monster)
        {
            BlackboxHandle.Of(this).Exert(monster, "몬스터 등록");

            if (!monster.IsValid())
                throw new ArgumentException(BlackboxHandle.Of(this).CrashExport(
                    Ctx("유효하지 않은 monster 인자가 입력되었습니다.")),
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
            using var _ = BlackboxHandle.Of(this).WriteScope($"Destroy, wasDestroyed: {_isDestroyed}");

            if (_isDestroyed) return;
            _isDestroyed = true;

            _monsters.ToList().ForEach(m =>
            {
                BlackboxHandle.Of(this).Exert(m, "몬스터 삭제");
                m.Destroy();
            });
            _monsters.Clear();
        }

        private string Ctx(string message) => $"[{nameof(MonsterManager)}] {message}";
    }
}
