using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class MonsterPlayerDetector : MonoBehaviour
    {
        // Front
        public event Action<GameObject> PlayerDetected;
        public GameObject CurrentPlayer { get; private set; }

        // Internal
        [SerializeField] private bool _setSizeByParentOnAwake = true;
        private BoxCollider2D _colliderComponent;


        // Content
        private void Awake()
        {
            _colliderComponent = GetComponent<BoxCollider2D>();

            if (_setSizeByParentOnAwake)
                SetSizeByParent();

        }

        /// <summary>
        /// 몬스터의 콜라이더 크기와 위치에 맞게 자신의 높이를 자동으로 조절합니다.
        /// </summary>
        internal void SetSize(float offset, float height)
        {
            if (!Application.isPlaying)
                _colliderComponent = GetComponent<BoxCollider2D>();

            if (!_colliderComponent)
                throw new InvalidOperationException(
                    Ctx("자신이 유효한 콜라이더를 가지고 있지 않습니다."));


            var factor = transform.lossyScale.z;

            _colliderComponent.offset = new Vector2
            {
                x = 0,
                y = offset / factor
            };

            _colliderComponent.size = new Vector2
            {
                x = _colliderComponent.size.x / factor,
                y = height / factor
            };
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player"))
                return;

            CurrentPlayer = collision.gameObject;
            PlayerDetected?.Invoke(CurrentPlayer);
        }

        private void OnTriggerExit2D(Collider2D collision)
        {
            if (!collision.CompareTag("Player"))
                return;

            CurrentPlayer = null;
        }

        private void SetSizeByParent()
        {
            var parentTransform = transform.parent;

            if (!parentTransform)
                throw new InvalidOperationException(
                    Ctx($"{nameof(MonsterPlayerDetectorEditor)}의 부모가 유효하지 않은 상태입니다."));

            if (!parentTransform.TryGetComponent<Collider2D>(out var parentCollider))
                throw new InvalidOperationException(
                    Ctx($"몬스터 {parentTransform.name}이(가) 유효한 콜라이더를 가지고 있지 않습니다."));


            SetSize(
                parentCollider.offset.y,
                parentCollider.bounds.max.y - parentCollider.bounds.min.y);
        }

        private string Ctx(string message) => $"[MonsterPlayerDetector] {message}";


#if UNITY_EDITOR
        [CustomEditor(typeof(MonsterPlayerDetector)), CanEditMultipleObjects]
        private class MonsterPlayerDetectorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Set Vertical Property"))
                    ((MonsterPlayerDetector)target).SetSizeByParent();
            }
        }
#endif
    }
}
