using System.Collections;
using System.Collections.Generic;
using Infrastructure;
using UnityEngine;

namespace Dialogue
{
    [CreateAssetMenu(fileName = "Dialogue Script", menuName = "Project H/Dialogue Script")]
    public class DialogueScriptSO : ScriptableObject, IReadOnlyList<DialogueLine>
    {
        [SerializeField] private string _title;
        [SerializeField] private Language _language;
        [Space]
        [SerializeField] private bool _showOneRandomLine;
        [SerializeField] private DialogueStyle _targetStyle;
        [Space]
        [SerializeField] private DialogueLine[] _lines;

        public string Title => _title;
        public bool ShowOneRandomLine => _showOneRandomLine;
        public DialogueStyle TargetStyle => _targetStyle;
        public DialogueLine[] Lines => _lines;

        public int Count => Lines?.Length ?? 0;
        public DialogueLine this[int index] => Lines[index];

        public IEnumerator<DialogueLine> GetEnumerator()
        {
            if (Lines == null)
                yield break;

            foreach (var line in Lines)
                yield return line;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
