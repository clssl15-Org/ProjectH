using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(BoxCollider2D))]
public class MonsterPlayerDetector : MonoBehaviour
{
    // Front
    public event Action<GameObject> PlayerDetected;
    public GameObject CurrentPlayer { get; private set; }

    // Internal
    private const string TargetTag = "Player";
    private BoxCollider2D colliderComponent;


    // Content
    private void Awake()
    {
        colliderComponent = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// 몬스터의 콜라이더 크기와 위치에 맞게 자신의 높이를 자동으로 조절합니다.
    /// </summary>
    internal void SetSize(float offset, float height)
    {
        if (!Application.isPlaying)
            colliderComponent = GetComponent<BoxCollider2D>();

        if (!colliderComponent)
            throw new InvalidOperationException(Ctx("자신이 유효한 콜라이더를 가지고 있지 않습니다."));


        colliderComponent.offset = new Vector2
        {
            x = 0,
            y = offset
        };

        colliderComponent.size = new Vector2
        {
            x = colliderComponent.size.x,
            y = height
        };
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(TargetTag))
            return;

        CurrentPlayer = collision.gameObject;
        PlayerDetected?.Invoke(CurrentPlayer);
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(TargetTag))
            return;

        CurrentPlayer = null;
    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MonsterPlayerDetector)), CanEditMultipleObjects]
    private class MonsterPlayerDetectorEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Set Vertical Property"))
            {
                var target = (MonsterPlayerDetector)base.target;
                var parentTransform = target.transform.parent;

                if (!parentTransform)
                    throw new InvalidOperationException(
                        target.Ctx("몬스터가 유효하지 않은 상태이기 때문에 행동을 수행할 수 없습니다."));

                if (!parentTransform.TryGetComponent<Collider2D>(out var parentCollider))
                    throw new InvalidOperationException(
                        target.Ctx($"몬스터 {parentTransform.name}이(가) 유효한 콜라이더를 가지고 있지 않습니다."));

                target.SetSize(
                    parentCollider.offset.y,
                    parentCollider.bounds.max.y - parentCollider.bounds.min.y);
            }
        }
    }
#endif

    private string Ctx(string message) => $"[MonsterPlayerDetector] {message}";
}
