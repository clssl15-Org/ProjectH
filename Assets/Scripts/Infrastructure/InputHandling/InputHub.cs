using System;
using System.Collections.Generic;
using System.Linq;
using BlackboxSystem;
using UnityEngine;

namespace Infrastructure
{
    public partial class InputHub : MonoBehaviour, IInputLayerHub
    {
        private readonly List<Layer> _layers = new();
        private readonly Dictionary<IInputLayerSubject, (Layer layer, Action destroyCallback)> _subjects = new();
        private readonly Dictionary<object, EmptyLayerSubject> _blockers = new();

        public void Add(IInputLayerSubject subject, bool blockBelows = true) => AddTo(string.Empty, subject, blockBelows);
        public void Add(IEnumerable<IInputLayerSubject> subjects, bool blockBelows = true) => AddTo(string.Empty, subjects, blockBelows);
        public void AddTo(string targetLayer, IInputLayerSubject subject, bool blockBelows = true) => AddTo(targetLayer, Enumerable.Repeat(subject, 1), blockBelows);
        public void AddTo(string targetLayer, IEnumerable<IInputLayerSubject> subjects, bool blockBelows = true)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Add, targetLayer: {targetLayer}");
            if (subjects == null) throw new ArgumentNullException(nameof(subjects));
            if (!subjects.Any()) return;

            foreach (var subject in subjects)
            {
                if (_subjects.ContainsKey(subject))
                    RemoveInternal(subject, IInputLayerHub.RemoveOption.RemoveIfEmpty, out var __);
            }

            var layer = !string.IsNullOrEmpty(targetLayer)
                ? _layers.FirstOrDefault(l => l.Name == targetLayer)
                : null;

            if (layer == null)
            {
                var layerName = !string.IsNullOrEmpty(targetLayer)
                    ? $"레이어 '{targetLayer}'"
                    : "빈 레이어";
                BlackboxHandle.Of(this).Write($"{layerName}을(를) 생성합니다.");

                layer = new Layer(targetLayer);
                _layers.Add(layer);
            }

            if (blockBelows)
                layer.BlocksBelow = true;

            foreach (var subject in subjects)
            {
                BlackboxHandle.Of(this).Exert(subject, "Add");

                layer.Add(subject);
                _subjects[subject] = (layer, () => Remove(subject));
                subject.Destroying += _subjects[subject].destroyCallback;
            }

            EvaluateInputState();   
        }

        public void Remove(
            IInputLayerSubject subject,
            IInputLayerHub.RemoveOption removeOption = IInputLayerHub.RemoveOption.RemoveIfEmpty,
            bool forceUnblock = false)
        {
            RemoveInternal(subject, removeOption, out var layer);

            if (forceUnblock)
            {
                BlackboxHandle.Of(this).Exert(layer, "Force Unblock");
                layer.Unblock();
                layer.BlocksBelow = false;
            }

            EvaluateInputState();
        }
        private void RemoveInternal(
            IInputLayerSubject subject,
            IInputLayerHub.RemoveOption removeOption,
            out Layer layer)
        {
            using var _ = BlackboxHandle.Of(this).ExertScope(subject, "Remove");
            if (!_subjects.TryGetValue(subject, out var subjectData))
            {
                BlackboxHandle.Of(this).Write(
                    $"{nameof(subject)} {subject}을(를) 가지고 있지 않기 때문에 Remove를 취소합니다.");
                layer = default;
                return;
            }

            var targetLayer = subjectData.layer;
            layer = targetLayer;
            Remove(subject, true);

            if ((removeOption == IInputLayerHub.RemoveOption.RemoveIfEmpty && targetLayer.IsEmpty)
                || removeOption == IInputLayerHub.RemoveOption.Forced)
            {
                foreach (var layerSubject in targetLayer.Subjects)
                    Remove(layerSubject, false);

                _layers.Remove(targetLayer);
            }

            void Remove(IInputLayerSubject subject, bool removeFromLayer)
            {
                subject.Destroying -= _subjects[subject].destroyCallback;
                _subjects.Remove(subject);

                if (removeFromLayer) targetLayer.Remove(subject);
                subject.AllowInput = true;
            }
        }

        public void Block(object requester)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Block: {requester}");
            if (requester == null || _blockers.ContainsKey(requester))
            {
                BlackboxHandle.Of(this).Write($"{nameof(requester)} {requester}은(는) 유효하지 않거나 이미 Block 상태입니다.");
                return;
            }

            _blockers[requester] = new EmptyLayerSubject();
            Add(_blockers[requester]);
        }

        public void Unblock(object requester)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Unblock: {requester}");
            if (requester == null || !_blockers.ContainsKey(requester))
            {
                BlackboxHandle.Of(this).Write($"{nameof(requester)} {requester}은(는) 유효하지 않거나 Block 상태가 아닙니다.");
                return;
            }

            Remove(_blockers[requester]);
            _blockers.Remove(requester);
        }

        private void EvaluateInputState()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Evaluate Input State");
            var block = false;

            for (int i = _layers.Count - 1; i >= 0; i--)
            {
                var layer = _layers[i];

                if (block)
                {
                    BlackboxHandle.Of(this).Exert(layer, "Block");
                    layer.Block();
                }
                else
                {
                    BlackboxHandle.Of(this).Exert(layer, "Unblock");
                    layer.Unblock();
                }

                if (layer.BlocksBelow)
                    block = true;
            }
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");

            foreach (var (subject, (_, callback)) in _subjects)
                subject.Destroying -= callback;
        }
    }
}
