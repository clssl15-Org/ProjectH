using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_5_3_OR_NEWER
namespace BlackboxSystem
{
    public class BlackboxLogExporter : MonoBehaviour
    {
        [SerializeField] private Object _target;

#if UNITY_EDITOR
        [CustomEditor(typeof(BlackboxLogExporter))]
        private class BlackboxLogExporterEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = ((BlackboxLogExporter)base.target)._target;

                GUILayout.Space(8);

                if (target == null)
                {
                    EditorGUILayout.HelpBox("Target must be assigned to enable Log Exporter", MessageType.Warning);
                    return;
                }

                if (Application.isPlaying)
                {
                    if (GUILayout.Button("Export Logs"))
                        BlackboxHandle.Of(target).Export();
                }
                else
                    GUILayout.Label("Enter Play Mode to export logs", EditorStyles.centeredGreyMiniLabel);
            }
        }
#endif
    }
}
#endif
