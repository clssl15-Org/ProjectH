using System;
using UnityEngine;

namespace World
{
    public class PortalDetector : MonoBehaviour
    {
        public event Action PlayerDetected;
        private bool _hasDetected;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (_hasDetected)
                return;

            if (collision.gameObject.CompareTag("Player"))
            {
                _hasDetected = true;
                PlayerDetected?.Invoke();
            }
        }
    }
}
