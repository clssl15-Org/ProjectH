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
        private bool _isDestroyed = false;


        // Content
        public bool Register(IMonster monster)
        {

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
