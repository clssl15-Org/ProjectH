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
        [SerializeField] private RectTransform _canvas;
        [SerializeField] private MonoBehaviour[] _exclusiveInputViews;

        // Internal
        private readonly HashSet<IViewModel> _viewModels = new();
        private readonly HashSet<IView> _views = new();
        private ViewInputHub _viewInputHub = new();
        private bool _isDestroyed = false;


        // Content
        private void Awake()
        {
            BlackboxHandle.Of(this).Write("Awake");
            
            if (!_canvas)
                throw new InvalidOperationException(BlackboxHandle.Of(this).CrashExport(
                    $"[{nameof(UIManager)}] {nameof(_canvas)} 컴포넌트가 유효하지 않습니다."));

            foreach (var viewObj in _exclusiveInputViews)
            {
                if (viewObj is not IEnablableView view)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).Write(
                        $"[{nameof(UIManager)}] exclusiveInputViews '{viewObj.name}'을(를) 등록하는 데 실패했습니다. " +
                        $"exclusiveInputViews는 {nameof(IEnablableView)}인 동시에 {nameof(IInputEnabledView)}(이)여야 합니다."));

                    continue;
                }
                if (viewObj is not IInputEnabledView iView)
                {
                    Debug.LogWarning(BlackboxHandle.Of(this).Write(
                        $"[{nameof(UIManager)}] exclusiveInputViews '{viewObj.name}'을(를) 등록하는 데 실패했습니다. " +
                        $"exclusiveInputViews는 {nameof(IEnablableView)}인 동시에 {nameof(IInputEnabledView)}(이)여야 합니다."));

                    continue;
                }

                view.Enabling += () =>
                {
                    if (_isDestroyed)
                        return;

                    BlackboxHandle.Of(view).Exerted(view, "view.Enabling");
                    BlackboxHandle.Of(view).Exert(_viewInputHub, "view.Enabling: Block Except Self");
                    _viewInputHub.BlockExcept(iView);
                };
                view.Disabling += () =>
                {
                    if (_isDestroyed)
                        return;

                    BlackboxHandle.Of(view).Exerted(view, "view.Disabling");
                    BlackboxHandle.Of(view).Exert(_viewInputHub, "view.Disabling: Unblock All");
                    _viewInputHub.UnblockAll();
                };

                RegisterView(iView);
            }
        }

        public void RegisterVM(IViewModel viewModel)
        {
            BlackboxHandle.Of(this).Exert(viewModel, "RegisterVM");

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
            BlackboxHandle.Of(this).Exert(view, "RegisterView");

            if (view == null)
                throw new ArgumentNullException(
                    nameof(view),
                    BlackboxHandle.Of(this).CrashExport(
                        Ctx("등록할 인자는 null일 수 없습니다.")));

            if (_views.Contains(view))
                return;

            _views.Add(view);
            view.Destroyed += () => _views.Remove(view);

            if (view is IInputEnabledView iView)
            {
                BlackboxHandle.Of(this).Exert(_viewInputHub, "RegisterView: view가 IInputEnabledView이기 떄문에 viewInputHub에 등록합니다.");
                _viewInputHub.Register(iView);
            }

            view.SetParent(_canvas);
        }

        private void OnDestroy() => Destroy();
        internal void Destroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;

            BlackboxHandle.Of(this).Write("Destroy");

            _views.ToList().ForEach(v =>
            {
                BlackboxHandle.Of(this).Exert(v, "Destroy: Destroy view");
                v.Destroy();
            });
            _viewModels.ToList().ForEach(vm =>
            {
                BlackboxHandle.Of(this).Exert(vm, "Destroy: Destroy viewModel");
                vm.Dispose();
            });

            _views.Clear();
            _viewModels.Clear();

            _viewInputHub = null;
        }

        private string Ctx(string message) => $"[{nameof(UIManager)}] {message}";
    }
}
