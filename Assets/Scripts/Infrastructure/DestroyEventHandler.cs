using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infrastructure
{
    public class DestroyEventHandler : MonoBehaviour
    {
        private readonly Dictionary<object, Action> _callbacks = new();

        public void Register(object key, Action callback) =>
            _callbacks[key] = callback;

        public void Remove(object key) =>
            _callbacks.Remove(key);

        private void OnDestroy()
        {
            foreach (var callback in _callbacks.Values)
                callback?.Invoke();

            _callbacks.Clear();
        }
    }
}
