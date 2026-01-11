using System;
using System.Collections.Generic;
using System.Linq;
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


        // Content
        private void Awake()
        {
            if (!_canvas)
                throw new InvalidOperationException(
                    $"[{nameof(UIManager)}] {nameof(_canvas)} 컴포넌트가 유효하지 않습니다.");

            foreach (var viewObj in _exclusiveInputViews)
            {
                if (viewObj is not IEnablableView view)
                {
                    Debug.LogWarning(
                        $"[{nameof(UIManager)}] exclusiveInputViews '{viewObj.name}'을(를) 등록하는 데 실패했습니다. " +
                        $"exclusiveInputViews는 {nameof(IEnablableView)}인 동시에 {nameof(IInputEnabledView)}(이)여야 합니다.");

                    continue;
                }
                if (viewObj is not IInputEnabledView iView)
                {
                    Debug.LogWarning(
                        $"[{nameof(UIManager)}] exclusiveInputViews '{viewObj.name}'을(를) 등록하는 데 실패했습니다. " +
                        $"exclusiveInputViews는 {nameof(IEnablableView)}인 동시에 {nameof(IInputEnabledView)}(이)여야 합니다.");

                    continue;
                }

                view.Enabling += () => _viewInputHub.BlockExcept(iView);
                view.Disabling += () => _viewInputHub.UnblockAll();

                RegisterView(iView);   
            }
        }

        public void RegisterVM(IViewModel viewModel)
        {
            if (viewModel == null)
                throw new ArgumentNullException(
                    nameof(viewModel),
                    Ctx("등록할 인자는 null일 수 없습니다."));

            if (_viewModels.Contains(viewModel))
                return;

            _viewModels.Add(viewModel);
            viewModel.Disposed += () => _viewModels.Remove(viewModel);
        }

        public void RegisterView(IView view)
        {
            if (view == null)
                throw new ArgumentNullException(
                    nameof(view),
                    Ctx("등록할 인자는 null일 수 없습니다."));

            if (_views.Contains(view))
                return;

            _views.Add(view);
            view.Destroyed += () => _views.Remove(view);

            if (view is IInputEnabledView iView)
                _viewInputHub.Register(iView);

            view.SetParent(_canvas);
        }


        internal void Destroy()
        {
            _viewInputHub = null;

            _views.ToList().ForEach(v => v.Destroy());
            _viewModels.ToList().ForEach(vm => vm.Dispose());

            _views.Clear();
            _viewModels.Clear();
        }

        private string Ctx(string message) => $"[{nameof(UIManager)}] {message}";
    }
}
