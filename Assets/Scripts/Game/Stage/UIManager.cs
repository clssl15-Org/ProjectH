using System;
using System.Collections.Generic;
using System.Linq;
using BlackboxSystem;
using UI;
using UnityEngine;

namespace Game.Stage
{
    public class UIManager : MonoBehaviour
    {
        // Bindings
        [SerializeField] internal RectTransform _canvas;
        public bool HasCanvas => _canvas != null;

        // Internal
        private readonly HashSet<IViewModel> _viewModels = new();
        private readonly HashSet<IView> _views = new();
        private bool _isDestroyed = false;


        // Content
        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");
            
            if (!_canvas)
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    $"[{nameof(UIManager)}] {nameof(_canvas)} 컴포넌트가 유효하지 않습니다."));
        }
        internal void SetCanvas(RectTransform canvas)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope($"Set Canvas: {canvas}");

            if (_canvas != null)
                Debug.Log(BlackboxHandle.Of(this).WriteMessage(Ctx(
                    $"캔버스를 교체합니다. '{canvas}' -> '{_canvas}'")));

            _canvas = canvas;
        }

        public void RegisterVM(IViewModel viewModel)
        {
            using var _ = BlackboxHandle.Of(this).ExertScope(viewModel, "RegisterVM");

            if (viewModel == null)
                throw new ArgumentNullException(
                    nameof(viewModel),
                    BlackboxHandle.Of(this).CrashExport(
                        Ctx("등록할 인자는 null일 수 없습니다.")));

            if (_viewModels.Contains(viewModel))
                return;

            _viewModels.Add(viewModel);
            viewModel.Disposed += () =>
            {
                if (!_isDestroyed)
                    _viewModels.Remove(viewModel);
            };
        }

        public void RegisterView(IView view)
        {
            using var _ = BlackboxHandle.Of(this).ExertScope(view, "RegisterView");

            if (view == null)
                throw new ArgumentNullException(
                    nameof(view),
                    BlackboxHandle.Of(this).CrashExport(
                        Ctx("등록할 인자는 null일 수 없습니다.")));

            if (_views.Contains(view))
                return;

            _views.Add(view);
            view.Destroying += () => _views.Remove(view);

            view.SetParent(_canvas);
        }

        private void OnDestroy() => Destroy();
        internal void Destroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            using var _ = BlackboxHandle.Of(this).WriteScope("Destroy");

            _views.ToList().ForEach(v =>
            {
                BlackboxHandle.Of(this).Exert(v, "Destroy view");
                v.Destroy();
            });
            _viewModels.ToList().ForEach(vm =>
            {
                BlackboxHandle.Of(this).Exert(vm, "Destroy viewModel");
                vm.Dispose();
            });

            _views.Clear();
            _viewModels.Clear();
        }

        private string Ctx(string message) => $"[{nameof(UIManager)}] {message}";
    }
}
