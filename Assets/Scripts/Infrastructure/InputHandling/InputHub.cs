using System;
using System.Collections.Generic;
using BlackThunder.BlackboxSystem;
using UnityEngine;

namespace Infrastructure
{
    public partial class InputHub : MonoBehaviour, IInputHub
    {
        private readonly List<IInputLayerSubject> _subjects = new();
        private readonly Dictionary<IInputLayerSubject, (bool isAwake, Action<bool> awakeChanged, Action onDestroy)> _subjectData = new();
        private readonly Dictionary<object, EmptyInputSubject> _blockers = new();
        private BlackboxHandle _blackbox;

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct("입력 허브 초기화를 시작합니다.", out _blackbox);
        }

        public void Add(IInputLayerSubject subject) => AddAfter(null, subject);
        public void AddAfter(IInputLayerSubject target, IInputLayerSubject subject)
        {
            using var _ = target != null
                ? _blackbox.Scope("입력 계층 대상을 등록합니다.").With(target, subject)
                : _blackbox.Scope("입력 계층 대상을 등록합니다.").With(subject);

            if (subject == null) throw new ArgumentNullException(nameof(subject));

            if (_subjects.Contains(subject))
            {
                _subjects.Remove(subject);
                _subjectData.Remove(subject);
            }


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

            if (subject == null)
                throw new ArgumentNullException(nameof(subject));
            if (!_subjects.Contains(subject))
                throw new ArgumentException(
                    "�������� �ʴ� subject�� WakeUp�Ϸ��� �õ��߽��ϴ�.");

            var (wasAwake, awakeChanged, onDestroy) = _subjectData[subject];

            _subjectData[subject] = (awake, awakeChanged, onDestroy);
            EvaluateInputState();
        }

        public void Remove(IInputLayerSubject subject)
        {
            using var _ = _blackbox.Scope("입력 계층 대상을 제거합니다.").With(subject);

            if (!_subjects.Contains(subject)) return;


            if (subject is IAwakableInputLayerSubject aSubject)
                aSubject.InputAwakeStateChanged -= _subjectData[subject].awakeChanged;

            subject.Destroying -= _subjectData[subject].onDestroy;
            _subjectData.Remove(subject);
            _subjects.Remove(subject);

            EvaluateInputState();
        }


        public void Block(object requester)
        {
            using var _ = _blackbox.Scope("입력 차단을 요청합니다.").With(requester);

            if (requester == null || _blockers.ContainsKey(requester))
            {
                return;
            }

            _blockers[requester] = new EmptyInputSubject(requester.ToString());
            Add(_blockers[requester]);
        }

        public void Unblock(object requester)
        {
            using var _ = _blackbox.Scope("입력 차단 해제를 요청합니다.").With(requester);

            if (requester == null || !_blockers.ContainsKey(requester))
            {
                return;
            }

            Remove(_blockers[requester]);
            _blockers.Remove(requester);
        }

        private void EvaluateInputState()
        {
            bool doBlock = false;

            for (int i = _subjects.Count - 1; i >= 0; i--)
            {
                var subject = _subjects[i];
                if (!_subjectData[subject].isAwake) continue;

                if (doBlock)
                {
                    subject.AllowInput = false;
                }
                else
                {
                    subject.AllowInput = true;
                }

                if (!subject.IsTrigger) doBlock = true;
            }
        }


        private void OnDestroy()
        {
            using var _ = _blackbox.Scope("입력 허브를 정리합니다.");

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
