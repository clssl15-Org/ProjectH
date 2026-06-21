using System;
using BlackThunder.BlackboxSystem;
using Game.Management;
using Infrastructure;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextAssigner : MonoBehaviour
    {
        [SerializeField] private UITextResource _resource;
        private TextMeshProUGUI _text;


        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct();

            _text = GetComponent<TextMeshProUGUI>();
            LanguageManager.LanguageChanged += AssignSprite;
        }

        private void Start()
        {
            AssignSprite(LanguageManager.Language);
        }

        private void AssignSprite(Language language)
        {
            using var _ = BlackboxHandle.Of(this).Scope($"Assign Sprite, lan: {language}");

            if (!this) return;
            if (_resource == UITextResource.None) return;

            try
            {
                _text.text = UITextLibrary.GetText(_resource, language);
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
            LanguageManager.LanguageChanged -= AssignSprite;
        }
    }
}
