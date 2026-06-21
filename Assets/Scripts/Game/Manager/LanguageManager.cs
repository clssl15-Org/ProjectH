using System;
using System.Globalization;
using UnityEngine;
using Infrastructure;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Game.Management
{
    public class LanguageManager : MonoBehaviour
    {
        public static Language Language { get; private set; }
        public static event Action<Language> LanguageChanged;

        [SerializeField] private Language _targetLanguage = Language.Korean;

        private void Awake() => SetLanguage(GetLanguageFromCurrentRegion(_targetLanguage));
        private static Language GetLanguageFromCurrentRegion(Language fallbackLanguage)
        {
            try
            {
                var region = RegionInfo.CurrentRegion;
                return string.Equals(region.TwoLetterISORegionName, "KR", StringComparison.OrdinalIgnoreCase)
                    ? Language.Korean
                    : Language.Engilsh;
            }
            catch (ArgumentException)
            {
                return fallbackLanguage;
            }
        }

        public static void SetLanguage(Language language)
        {
            if (Language == language) return;
            Language = language;

            LanguageChanged?.Invoke(Language);
        }

        public static void SetNextLanguage(bool next)
        {
            var languages = (Language[])Enum.GetValues(typeof(Language));
            var currentIndex = Array.IndexOf(languages, Language);
            var direction = next ? 1 : -1;

            for (int offset = 1; offset <= languages.Length; offset++)
            {
                var nextLanguage = languages[(currentIndex + (offset * direction) + languages.Length) % languages.Length];
                if (nextLanguage == Language.None || nextLanguage == Language.Undefined)
                    continue;

                SetLanguage(nextLanguage);
                return;
            }
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
                    LanguageManager.SetLanguage(target._targetLanguage);
                }
            }
        }
#endif
    }
}
