using System;
using System.Collections.Generic;
using System.Linq;
using Infrastructure;
using UnityEngine;

namespace Dialogue
{
    public class DialogueScriptLibrary : MonoBehaviour
    {
        [field: SerializeField] public DialogueScriptSO[] DialogueScripts { get; internal set; }
        public IEnumerable<string> AllScriptTitles => DialogueScripts?.Select(s => s.Title) ?? Array.Empty<string>();

        public bool TryGetDialogueScript(string title, Language language, out DialogueScriptSO script)
        {
            script = DialogueScripts.FirstOrDefault(s =>
                string.Equals(s.Title, title) &&
                s.Language == language);
            return script != null;
        }
    }
}
