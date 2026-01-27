using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace World
{
    public class DialogueScriptLibrary : MonoBehaviour
    {
        [field: SerializeField] public DialogueScriptSO[] DialogueScripts { get; internal set; }
        public IEnumerable<string> AllScriptTitles => DialogueScripts?.Select(s => s.Title) ?? Array.Empty<string>();

        public bool TryGetDialogueScript(string title, out DialogueScriptSO script)
        {
            script = DialogueScripts.FirstOrDefault(s => s.Title == title);
            return script != null;
        }
    }
}
