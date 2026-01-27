using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "Dialogue Script", menuName = "Project H/Dialogue Script")]
    public class DialogueScriptSO : ScriptableObject, IReadOnlyList<DialogueData>
    {
        [field: SerializeField] public string Title { get; internal set; }
        [field: SerializeField] public DialogueData[] Lines { get; internal set; }

        public int Count => Lines?.Length ?? 0;
        public DialogueData this[int index] => Lines[index];

        public IEnumerator<DialogueData> GetEnumerator()
        {
            if (Lines == null) yield break;
            foreach (var line in Lines)
                yield return line;
        }
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}