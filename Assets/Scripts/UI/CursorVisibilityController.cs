using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public sealed class CursorVisibilityController
    {
        private readonly HashSet<object> _visibleRequesters = new();
        private bool _isRestored;

        public void RequestVisible(object requester)
        {
            if (_isRestored || requester == null)
                return;

            _visibleRequesters.Add(requester);
            ApplyGameplayPolicy();
        }

        public void ReleaseVisible(object requester)
        {
            if (_isRestored || requester == null)
                return;

            _visibleRequesters.Remove(requester);
            ApplyGameplayPolicy();
        }

        public void ResetToGameplay()
        {
            _isRestored = false;
            _visibleRequesters.Clear();
            ApplyGameplayPolicy();
        }

        public void RestoreVisible()
        {
            _isRestored = true;
            _visibleRequesters.Clear();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ApplyGameplayPolicy()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = _visibleRequesters.Count > 0;
        }
    }

    public interface ICursorVisibilityControllerUser
    {
        void Initialize(CursorVisibilityController cursorVisibilityController);
    }
}
