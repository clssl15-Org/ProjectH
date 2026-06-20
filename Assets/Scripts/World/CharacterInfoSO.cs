using System;
using Infrastructure;
using UnityEngine;

namespace World
{
    [CreateAssetMenu(fileName = "Character Info", menuName = "Project H/Character Info")]
    public class CharacterInfoSO : ScriptableObject
    {
        [Serializable]
        public struct CharacterLocalizedName
        {
            [SerializeField] private Language language;
            [SerializeField] private string name;

            public CharacterLocalizedName(Language language, string name)
            {
                this.language = language;
                this.name = name;
            }

            public readonly Language Language => language;
            public readonly string Name => name;
        }

        private const Language DefaultLanguage = Language.Korean;

        [field: SerializeField] public Character Character { get; set; }
        [SerializeField] private CharacterLocalizedName[] localizedNames;
        [field: SerializeField] public Sprite Portrait { get; set; }

        public string Name
        {
            get => GetName(DefaultLanguage);
            set => SetNameForAllLanguages(value);
        }

        public string GetName(Language language) => GetLocalizedName(language).Name;

        public void SetName(Language language, string name)
        {
            var targetLanguage = NormalizeLanguage(language);

            if (localizedNames == null)
                localizedNames = Array.Empty<CharacterLocalizedName>();

            for (var i = 0; i < localizedNames.Length; i++)
            {
                if (localizedNames[i].Language != targetLanguage)
                    continue;

                localizedNames[i] = new CharacterLocalizedName(targetLanguage, name);
                return;
            }

            Array.Resize(ref localizedNames, localizedNames.Length + 1);
            localizedNames[localizedNames.Length - 1] = new CharacterLocalizedName(targetLanguage, name);
        }

        private void SetNameForAllLanguages(string name)
        {
            if (localizedNames == null || localizedNames.Length == 0)
            {
                SetName(DefaultLanguage, name);
                return;
            }

            var hasDefaultLanguage = false;
            for (var i = 0; i < localizedNames.Length; i++)
            {
                var language = NormalizeLanguage(localizedNames[i].Language);
                if (language == DefaultLanguage)
                    hasDefaultLanguage = true;

                localizedNames[i] = new CharacterLocalizedName(language, name);
            }

            if (!hasDefaultLanguage)
                SetName(DefaultLanguage, name);
        }

        private CharacterLocalizedName GetLocalizedName(Language language)
        {
            if (localizedNames == null || localizedNames.Length == 0)
                throw new InvalidOperationException(
                    Ctx("언어별 캐릭터 이름이 할당되지 않았습니다."));

            var targetLanguage = NormalizeLanguage(language);
            foreach (var localizedName in localizedNames)
            {
                if (localizedName.Language == targetLanguage)
                    return localizedName;
            }

            throw new InvalidOperationException(
                Ctx($"{targetLanguage} 언어 캐릭터 이름을 찾을 수 없습니다."));
        }

        private static Language NormalizeLanguage(Language language)
        {
            if (language == Language.None || language == Language.Undefined)
                return DefaultLanguage;

            return language;
        }

        private string Ctx(string message) => $"[{nameof(CharacterInfoSO)}:{name}] {message}";
    }
}
