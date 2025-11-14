using System;
using System.Collections.Generic;
using Actors;
using UnityEngine;

namespace Game
{
    [DisallowMultipleComponent]
    public class MonsterManager : MonoBehaviour
    {
        // Internal
        private readonly List<IMonster> _monsters = new();

        // Content
        public bool Register(IMonster monster)
        {
            if (!monster.IsValid())
                throw new ArgumentException(Ctx(
                    "유효하지 않은 인자가 입력되었습니다."), nameof(monster));

            if (_monsters.Contains(monster))
                return false;

            _monsters.Add(monster);
            monster.Destroyed += () => _monsters.Remove(monster);

            return true;
        }

        private void Destroy()
        {
            foreach (var monster in _monsters)
                monster.Destroy();

            _monsters.Clear();
        }

        private string Ctx(string message) => $"[{nameof(MonsterManager)}]: {message}";
    }
}
