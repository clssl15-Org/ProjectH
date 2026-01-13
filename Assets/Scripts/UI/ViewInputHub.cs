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
            using var _ = BlackboxHandle.Of(this).WriteScope("BlockAll");

            foreach (var view in _views.Keys)
            {
                BlackboxHandle.Of(this).Exert(view, "Block");
                view.EnableInput = false;
            }
        }
        public void BlockExcept(params IInputEnabledView[] views)
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("BlockExcept");

            foreach (var view in _views.Keys)
            {
                if (!views.Contains(view))
                {
                    BlackboxHandle.Of(this).Exert(view, "Block");
                    view.EnableInput = false;
                }
            }
        }

        public void UnblockAll()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("UnblockAll");

            foreach (var view in _views.Keys)
            {
                BlackboxHandle.Of(this).Exert(view, "Unblock");
                view.EnableInput = true;
            }
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
