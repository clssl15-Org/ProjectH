#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary>
/// 인스펙터에서 N개 유물을 강제 추가하는 디버그 도구. 첫 3개는 001·002·003(선봉의 창문, 단죄의 검식, 맹세의 깃발 조각) 고정.
/// </summary>
public class RelicDebug : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    private int relicCount = 5;

    public void AddRandomRelics()
    {
        if (RelicManager.Instance == null)
        {
            Debug.LogWarning("[RelicDebug] RelicManager.Instance가 없습니다. 플레이 모드에서 RelicManager가 씬에 있는지 확인하세요.", this);
            return;
        }

        RelicManager.Instance.DebugAddRandomRelics(relicCount);
        Debug.Log($"[RelicDebug] 유물 {relicCount}개 추가 요청 완료 (첫 3개: 001·002·003 고정).", this);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(RelicDebug))]
public class RelicDebugEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RelicDebug script = (RelicDebug)target;
        SerializedProperty countProp = serializedObject.FindProperty("relicCount");
        int count = countProp != null ? countProp.intValue : 0;

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "첫 3개는 001 선봉의 창문, 002 단죄의 검식, 003 맹세의 깃발 조각이 고정됩니다.\n" +
            "4번째부터는 등록된 유물 중 랜덤으로 추가됩니다.\n" +
            "각 유물마다 동전 앞면(강화)/뒷면(일반)이 50% 확률로 적용됩니다.",
            MessageType.Info);

        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button($"랜덤 유물 {count}개 추가"))
            script.AddRandomRelics();
        GUI.enabled = true;

        if (!Application.isPlaying)
            EditorGUILayout.HelpBox("플레이 모드에서만 동작합니다.", MessageType.Warning);
    }
}
#endif
