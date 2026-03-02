using System;
using System.Collections.Generic;
using BlackboxSystem;
using UnityEngine;

namespace Infrastructure
{
    public partial class InputHub : MonoBehaviour, IInputHub
    {
        private readonly List<IInputLayerSubject> _subjects = new();
        private readonly Dictionary<IInputLayerSubject, (bool isAwake, Action<bool> awakeChanged, Action onDestroy)> _subjectData = new();
        private readonly Dictionary<object, EmptyInputSubject> _blockers = new();

        public void Add(IInputLayerSubject subject) => AddAfter(null, subject);
        public void AddAfter(IInputLayerSubject target, IInputLayerSubject subject)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Add");
            if (subject == null) throw new ArgumentNullException(nameof(subject));

            if (_subjects.Contains(subject))
            {
                _subjects.Remove(subject);
                _subjectData.Remove(subject);
            }

            BlackboxHandle.Of(this).Exert(subject, $"Add, after: {target}");

            var idx = _subjects.Contains(target) ? _subjects.IndexOf(target) + 1 : _subjects.Count;
            _subjects.Insert(idx, subject);

            if (subject is IAwakableInputLayerSubject aSubject)
            {
                _subjectData[subject] = (true, aw => SetAwake(subject, aw), () => Remove(subject));
                aSubject.InputAwakeStateChanged += _subjectData[subject].awakeChanged;
            }
            else
                _subjectData[subject] = (true, null, () => Remove(subject));

            subject.Destroying += _subjectData[subject].onDestroy;

            EvaluateInputState();   
        }

        private void SetAwake(IInputLayerSubject subject, bool awake)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Set Awake");

            if (subject == null)
                throw new ArgumentNullException(nameof(subject));
            if (!_subjects.Contains(subject))
                throw new ArgumentException(
                    BlackboxHandle.Of(this).WriteError("존재하지 않는 subject를 WakeUp하려고 시도했습니다."));

            var (wasAwake, awakeChanged, onDestroy) = _subjectData[subject];
            BlackboxHandle.Of(this).Exert(subject, $"Set Sleep, {wasAwake} -> {awake}");

            _subjectData[subject] = (awake, awakeChanged, onDestroy);
            EvaluateInputState();
        }

        public void Remove(IInputLayerSubject subject)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Remove");
            if (!_subjects.Contains(subject)) return;

            BlackboxHandle.Of(this).Exert(subject, "Remove");

            if (subject is IAwakableInputLayerSubject aSubject)
                aSubject.InputAwakeStateChanged -= _subjectData[subject].awakeChanged;

            subject.Destroying -= _subjectData[subject].onDestroy;
            _subjectData.Remove(subject);
            _subjects.Remove(subject);

            EvaluateInputState();
        }


        public void Block(object requester)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Block: {requester}");
            if (requester == null || _blockers.ContainsKey(requester))
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(requester)} '{requester}'은(는) 유효하지 않거나 이미 Block 상태입니다.");
                return;
            }

            _blockers[requester] = new EmptyInputSubject(requester.ToString());
            Add(_blockers[requester]);
        }

        public void Unblock(object requester)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Unblock: {requester}");
            if (requester == null || !_blockers.ContainsKey(requester))
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(requester)} '{requester}'은(는) 유효하지 않거나 Block 상태가 아닙니다.");
                return;
            }

            Remove(_blockers[requester]);
            _blockers.Remove(requester);
        }

        private void EvaluateInputState()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Evaluate Input State");
            bool doBlock = false;

            for (int i = _subjects.Count - 1; i >= 0; i--)
            {
                var subject = _subjects[i];
                if (!_subjectData[subject].isAwake) continue;

                if (doBlock)
                {
                    BlackboxHandle.Of(this).Exert(subject, "Block");
                    subject.AllowInput = false;
                }
                else
                {
                    BlackboxHandle.Of(this).Exert(subject, "Unblock");
                    subject.AllowInput = true;
                }

                if (!subject.IsTrigger) doBlock = true;
            }
        }


        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");

            foreach (var (subject, (_, awakeChanged, onDestroy)) in _subjectData)
            {
                if (subject is IAwakableInputLayerSubject aSubject)
                    aSubject.InputAwakeStateChanged -= awakeChanged;

                subject.Destroying -= onDestroy;
            }

            _subjects.Clear();
            _subjectData.Clear();
            _blockers.Clear();
        }
    }
}
