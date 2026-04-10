using System;
using UnityEngine;

namespace World
{
    public class BoxDetector : MonoBehaviour
    {
        public bool IsDetected { get; private set; }
        public event Action PlayerDetected;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (IsDetected)
                return;

            if (collision.gameObject.CompareTag("Player"))
            {
                IsDetected = true;
                PlayerDetected?.Invoke();
            }
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!IsDetected)
                return;

            if (collision.gameObject.CompareTag("Player"))
                IsDetected = false;
        }
    }
}
