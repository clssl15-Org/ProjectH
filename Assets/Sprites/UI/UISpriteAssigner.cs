using System;
using BlackThunder.BlackboxSystem;
using Game.Management;
using Infrastructure;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Image))]
    public class UISpriteAssigner : MonoBehaviour
    {
        [SerializeField] private UISpriteResource _resource;
        [SerializeField] private bool _setNativeSize;

        private Image _image;


        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct();

            _image = GetComponent<Image>();
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
            if (_resource == UISpriteResource.None) return;

            try
            {
                _image.sprite = UISpriteLibrary.GetSprite(_resource, language);
                if (_setNativeSize) _image.SetNativeSize();
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
