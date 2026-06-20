using System;
using UnityEngine;
using Infrastructure;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Management
{
    public class LanguageManager : MonoBehaviour
    {
        public Language Language { get; private set; }
        public event Action<Language> LanguageChanged;

        private Language _language = Language.None;
        [SerializeField] private Language _targetLanguage = Language.Korean;

        public void SetLanguage(Language language)
        {
            if (_language == language) return;
            _language = language;

            LanguageChanged?.Invoke(_language);
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(LanguageManager)), CanEditMultipleObjects]
        protected class LanguageManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Apply"))
                {
                    var target = (LanguageManager)base.target;
                    target.SetLanguage(target._targetLanguage);
                }
            }
        }
#endif
    }
}
