using System;
using System.Collections.Generic;
using System.Linq;
using Actors;
using UnityEngine;

namespace Game.Stage
{
    public class MonsterManager : MonoBehaviour
    {
        // Internal
        private readonly HashSet<IMonster> _monsters = new();


        // Content
        public bool Register(IMonster monster)
        {
            if (!monster.IsValid())
                throw new ArgumentException(
                    Ctx("유효하지 않은 인자가 입력되었습니다."),
                    nameof(monster));

            if (_monsters.Contains(monster))
                return false;

            _monsters.Add(monster);
            monster.Destroyed += () => _monsters.Remove(monster);

            return true;
        }

        internal void Destroy()
        {
            _monsters.ToList().ForEach(m => m.Destroy());
            _monsters.Clear();
        }

        private string Ctx(string message) => $"[{nameof(MonsterManager)}] {message}";
    }
}
