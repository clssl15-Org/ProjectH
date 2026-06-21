using System;
using BlackThunder.BlackboxSystem;
using Game.Management;
using Infrastructure;
using TMPro;
using UnityEngine;
using World;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class SetCharacterNameText : MonoBehaviour
    {
        [SerializeField] private CharacterInfoSO _resource;
        private TextMeshProUGUI _text;

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct();

            _text = GetComponent<TextMeshProUGUI>();
            LanguageManager.LanguageChanged += AssignName;
        }

        private void Start()
        {
            AssignName(LanguageManager.Language);
        }

        private void AssignName(Language language)
        {
            using var _ = BlackboxHandle.Of(this).Scope($"Assign Name, lan: {language}");
            if (!this) return;

            try
            {
                _text.text = _resource.GetName(language);
            }
            catch (Exception ex)
            {
                BlackboxHandle.Of(this).CrashExport(ex);
                Debug.LogException(ex);
            }
        }

        private void OnDestroy()
        {
            using var _ = BlackboxHandle.Of(this).Scope("OnDestroy");
            LanguageManager.LanguageChanged -= AssignName;
        }
    }
}
