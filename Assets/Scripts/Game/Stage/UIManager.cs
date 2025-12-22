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

        // Internal
        private readonly HashSet<IViewModel> _viewModels = new();
        private readonly HashSet<IView> _views = new();


        // Content
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

            view.SetParent(_canvas);
        }


        internal void Destroy()
        {
            _views.ToList().ForEach(v => v.Destroy());
            _viewModels.ToList().ForEach(vm => vm.Dispose());

            _views.Clear();
            _viewModels.Clear();
        }

        private string Ctx(string message) => $"[{nameof(UIManager)}] {message}";
    }
}
