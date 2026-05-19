using Infrastructure;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Stage
{
    public enum StageDebugDestination
    {
        LargeMap,
        Boss,
        Room,
    }

    public class StageDebug : MonoBehaviour
    {
        [SerializeField] private int targetStage;
        [SerializeField] private StageDebugDestination destination = StageDebugDestination.LargeMap;
        [SerializeField] private int roomNumber = 1;

        public void GoToStage()
        {
            if (!DebugTools.IsDebugMode)
            {
                Debug.LogWarning("[StageDebug] DEBUG_MODE가 정의되지 않아 스테이지 이동을 사용할 수 없습니다.", this);
                return;
            }

            if (LevelManager.Instance == null)
            {
                Debug.LogWarning("[StageDebug] LevelManager.Instance가 null입니다.", this);
                return;
            }

            if (!TryResolveSceneName(out string sceneName))
                return;

            LevelManager.Instance.LoadNextScene(sceneName);
            Debug.Log($"[StageDebug] '{sceneName}' 씬으로 이동합니다. (Stage {targetStage})", this);
        }

        private bool TryResolveSceneName(out string sceneName)
        {
            sceneName = null;
            targetStage = Mathf.Clamp(targetStage, 0, 3);

            switch (destination)
            {
                case StageDebugDestination.LargeMap:
                    sceneName = targetStage == 0 ? "Stage0 0" : $"Stage{targetStage}_LargeMap";
                    return true;
                case StageDebugDestination.Boss:
                    if (targetStage == 0)
                    {
                        Debug.LogWarning("[StageDebug] Stage 0에는 보스 씬이 없습니다.", this);
                        return false;
                    }
                    sceneName = $"Stage{targetStage}Boss";
                    return true;
                case StageDebugDestination.Room:
                    roomNumber = Mathf.Clamp(roomNumber, 0, 10);
                    sceneName = $"Stage{targetStage} {roomNumber}";
                    return true;
                default:
                    return false;
            }
        }
    }

#if UNITY_EDITOR
    [CustomEditor(typeof(StageDebug))]
    public class StageDebugEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            GUILayout.Space(8);

            if (!DebugTools.IsDebugMode)
            {
                EditorGUILayout.HelpBox("DEBUG_MODE 스크립팅 심볼이 필요합니다.", MessageType.Info);
                return;
            }

            var stageDebug = (StageDebug)target;
            if (GUILayout.Button("Go To Stage"))
                stageDebug.GoToStage();

            if (!Application.isPlaying)
                EditorGUILayout.HelpBox("플레이 모드에서만 씬 전환이 동작합니다.", MessageType.Info);
        }
    }
#endif
}
