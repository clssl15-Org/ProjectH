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
        [SerializeField] internal RectTransform _canvas;
        [SerializeField] internal RectTransform _worldUI;
        public bool HasCanvas => _canvas != null;
        public bool HasWorldUI => _worldUI != null;

        // Internal
        private readonly HashSet<IViewModel> _viewModels = new();
        private readonly HashSet<IView> _views = new();
        private bool _isDestroyed = false;


        // Content
        private void Awake()
        {
            
            if (!_canvas)
                throw new InvalidOperationException($"[{nameof(UIManager)}] {nameof(_canvas)} ������Ʈ�� ��ȿ���� �ʽ��ϴ�.");
        }
        internal void SetCanvas(RectTransform canvas)
        {

            if (_canvas != null)
                Debug.Log(Ctx(
                    $"ĵ������ ��ü�մϴ�. '{_canvas}' -> '{canvas}'"));
            _canvas = canvas;
        }
        internal void SetWorldUI(RectTransform worldUI)
        {

            if (_worldUI != null)
                Debug.Log(Ctx(
                    $"WorldUI�� ��ü�մϴ�. '{_worldUI}' -> '{worldUI}'"));
            _worldUI = worldUI;
        }

        public void RegisterVM(IViewModel viewModel)
        {

            if (viewModel == null)
                throw new ArgumentNullException(
                    nameof(viewModel),
                    Ctx("����� ���ڴ� null�� �� �����ϴ�."));

            if (_viewModels.Contains(viewModel))
                return;

            _viewModels.Add(viewModel);
            viewModel.Disposed += () =>
            {
                if (!_isDestroyed)
                    _viewModels.Remove(viewModel);
            };
        }

        public void RegisterView(IView view, bool worldUIParent = false)
        {

            if (view == null)
                throw new ArgumentNullException(
                    nameof(view),
                    Ctx("����� ���ڴ� null�� �� �����ϴ�."));

            if (_views.Contains(view))
                return;

            _views.Add(view);
            view.Destroying += () => _views.Remove(view);

            view.SetParent(worldUIParent ? _worldUI : _canvas);
        }

        private void OnDestroy() => Destroy();
        internal void Destroy()
        {
            if (_isDestroyed) return;
            _isDestroyed = true;


            _views.ToList().ForEach(v =>
            {
                v.Destroy();
            });
            _viewModels.ToList().ForEach(vm =>
            {
                vm.Dispose();
            });

            _views.Clear();
            _viewModels.Clear();
        }

        private string Ctx(string message) => $"[{nameof(UIManager)}] {message}";
    }
}
