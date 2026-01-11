using System;
using System.Collections.Generic;
using System.Linq;
using BlackboxSystem;

namespace UI
{
    internal class ViewInputHub
    {
        private readonly Dictionary<IInputEnabledView, Action> _views = new();


        public void BlockAll()
        {
            foreach (var view in _views.Keys)
                view.EnableInput = false;
        }
        public void BlockExcept(params IInputEnabledView[] views)
        {
            foreach (var view in _views.Keys)
            {
                if (!views.Contains(view))
                    view.EnableInput = false;
            }
        }

        public void UnblockAll()
        {
            foreach (var view in _views.Keys)
                view.EnableInput = true;
        }


        public void Register(IInputEnabledView view)
        {
            if (view == null)
                throw new ArgumentNullException(
                    nameof(view),
                    $"[{nameof(ViewInputHub)}] 등록할 {nameof(view)}은(는) null일 수 없습니다.");

            if (_views.ContainsKey(view))
                return;

            _views[view] = () => Remove(view);
            view.Destroyed += _views[view];
        }

        public void Remove(IInputEnabledView view)
        {
            if (!_views.ContainsKey(view))
                return;

            view.Destroyed -= _views[view];
            _views.Remove(view);
        }
    }
}
