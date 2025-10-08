using UnityEditor;
using UnityEngine;

namespace Infrastructure
{
    [RequireComponent(typeof(SpriteRenderer), typeof(BoxCollider2D))]
    public class ColliderSizeHandler : MonoBehaviour
    {
        [SerializeField, Tooltip("이 항목을 활성화하면 게임 시작 시에 콜라이더 크기를 스프라이트 크기에 맞춥니다.")]
        private bool _setColliderSizeOnStart = false;


        private void Awake()
        {
            if (_setColliderSizeOnStart)
                SetColliderSize();
        }

        /// <summary>
        /// 이 메서드를 호출하여 콜라이더 크기를 조절하십시오.
        /// </summary>
        public void SetColliderSize()
        {
            var sr = GetComponent<SpriteRenderer>();
            if (!sr.sprite)
            {
                throw new System.InvalidOperationException(
                    $"GameObject {name}에 할당된 sprite가 없으므로 SetColliderSize 메서드를 실행할 수 없습니다.");
            }

            var col = sr.GetComponent<BoxCollider2D>();
            col.size = sr.bounds.size;
        }


        [CustomEditor(typeof(ColliderSizeHandler))]
        private class CubeGenerateButton : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Set Collider Size"))
                    ((ColliderSizeHandler)target).SetColliderSize();
            }
        }
    }
}
