using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Dialogue
{
    public class DialogueScriptLibrary : MonoBehaviour
    {
        [field: SerializeField] public DialogueScriptSO[] DialogueScripts { get; internal set; }
        public IEnumerable<string> AllScriptTitles => DialogueScripts?.Select(s => s.Title) ?? Array.Empty<string>();

        public bool TryGetDialogueScript(DialogueTitle title, out DialogueScriptSO script) =>
            TryGetDialogueScript(title.ToString(), out script);
        public bool TryGetDialogueScript(string title, out DialogueScriptSO script)
        {
            script = DialogueScripts.FirstOrDefault(s => string.Equals(s.Title, title));
            return script != null;
        }
    }
}
