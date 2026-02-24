using System.Collections.Generic;
using BlackboxSystem;
using UnityEngine;

namespace Infrastructure
{
    public partial class InputHub
    {
        private class Layer
        {
            public string Name { get; }
            public bool BlocksBelow { get; set; }

            public IReadOnlyList<IInputLayerSubject> Subjects => _subjects;
            public bool IsEmpty => _subjects.Count == 0;

            private readonly List<IInputLayerSubject> _subjects = new();
            private bool _isBlocked = false;


            public Layer(string name) => Name = name;

            public void Add(IInputLayerSubject subject)
            {
                using var _ = BlackboxHandle.Of(this).ExertScope(subject, $"Add: {subject}");

                if (_subjects.Contains(subject))
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).WriteMessage(
                        $"이미 등록된 Subject '{subject}'을(를) {(!string.IsNullOrWhiteSpace(Name) ? (Name + "레이어에 ") : "")}재등록할 수 없습니다. " +
                        $"행동을 취소합니다."));
                    return;
                }

                _subjects.Add(subject);
                subject.AllowInput = !_isBlocked;
            }

            public void Remove(IInputLayerSubject subject)
            {
                using var _ = BlackboxHandle.Of(this).ExertScope(subject, $"Remove: {subject}");
                _subjects.Remove(subject);
            }

            public void Block()
            {
                using var _ = BlackboxHandle.Of(this).WriteScope($"Block, isBlocked: {_isBlocked}");

                if (_isBlocked) return;
                _isBlocked = true;

                foreach (var subject in _subjects)
                {
                    BlackboxHandle.Of(this).Exert(subject, "Block");
                    subject.AllowInput = false;
                }
            }

            public void Unblock()
            {
                using var _ = BlackboxHandle.Of(this).WriteScope($"Unblock, isBlocked: {_isBlocked}");

                if (!_isBlocked) return;
                _isBlocked = false;

                foreach (var subject in _subjects)
                {
                    BlackboxHandle.Of(this).Exert(subject, "Unblock");
                    subject.AllowInput = true;
                }
            }

            public override string ToString() => $"Layer '{Name}' (Subjects: {_subjects.Count}, BlocksBelow: {BlocksBelow}, IsBlocked: {_isBlocked})";
        }
    }
}
