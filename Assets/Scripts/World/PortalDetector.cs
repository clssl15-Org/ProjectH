using System;
using UnityEngine;

namespace World
{
    public class PortalDetector : MonoBehaviour
    {
        public bool IsDetected { get; private set; }
        public event Action PlayerDetected;

        private bool _hasDetected;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;

            IsDetected = true;

            if (_hasDetected)
                return;

            _hasDetected = true;
            PlayerDetected?.Invoke();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.gameObject.CompareTag("Player"))
                return;

            IsDetected = false;
        }
    }
}
