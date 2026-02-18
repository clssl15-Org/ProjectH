using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dialogue
{
    [CreateAssetMenu(fileName = "Dialogue Script", menuName = "Project H/Dialogue Script")]
    public class DialogueScriptSO : ScriptableObject, IReadOnlyList<DialogueLine>
    {
        [SerializeField] private DialogueTitle _dialogueTitle;
        [SerializeField] private string _title;
        [SerializeField] private DialogueStyle _targetDialogueStyle;
        [Space]
        [SerializeField] private DialogueLine[] _lines;

        public DialogueTitle DialogueTitle => _dialogueTitle;
        public string Title => _dialogueTitle != DialogueTitle.Undefined ? _dialogueTitle.ToString() : _title;
        public DialogueStyle TargetDialogueStyle => _targetDialogueStyle;
        public DialogueLine[] Lines => _lines;

        public int Count => Lines?.Length ?? 0;
        public DialogueLine this[int index] => Lines[index];

        public IEnumerator<DialogueLine> GetEnumerator()
        {
            if (Lines == null) yield break;
            foreach (var line in Lines)
                yield return line;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


#if UNITY_EDITOR
        [CustomEditor(typeof(DialogueScriptSO)), CanEditMultipleObjects]
        private class DialogueScriptSOEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                serializedObject.Update();
                var target = (DialogueScriptSO)base.target;

                if (target.DialogueTitle != DialogueTitle.Undefined)
                    DrawPropertiesExcluding(serializedObject, nameof(_title));
                else
                    DrawDefaultInspector();

                serializedObject.ApplyModifiedProperties();
            }
        }
#endif
    }
}
