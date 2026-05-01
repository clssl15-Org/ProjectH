using System;
using UnityEngine;

namespace World
{
    public class PortalDetector : MonoBehaviour
    {
        public bool IsDetected { get; private set; }
        public bool CanNotifyPlayerDetected { get; set; } = true;
        public event Action PlayerDetected;

        private bool _hasNotified;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsDetected || !collision.gameObject.CompareTag("Player"))
                return;

            IsDetected = true;

            if (!CanNotifyPlayerDetected || _hasNotified)
                return;

            _hasNotified = true;
            PlayerDetected?.Invoke();
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!IsDetected || !collision.gameObject.CompareTag("Player"))
                return;

            IsDetected = false;
        }
    }
}
