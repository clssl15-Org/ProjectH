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

        [field: SerializeField] public Character Character { get; set; }
        [SerializeField] private CharacterLocalizedName[] localizedNames;
        [field: SerializeField] public Sprite Portrait { get; set; }

        public string GetName(Language language)
        {
            if (localizedNames == null || localizedNames.Length == 0)
                throw new InvalidOperationException(
                    Ctx("언어별 캐릭터 이름이 할당되지 않았습니다."));

            foreach (var localizedName in localizedNames)
            {
                if (localizedName.Language == language)
                    return localizedName.Name;
            }

            throw new InvalidOperationException(
                Ctx($"{language} 언어 캐릭터 이름을 찾을 수 없습니다."));
        }

        public void SetName(Language language, string name)
        {
            localizedNames ??= Array.Empty<CharacterLocalizedName>();

            for (var i = 0; i < localizedNames.Length; i++)
            {
                if (localizedNames[i].Language != language)
                    continue;

                localizedNames[i] = new CharacterLocalizedName(language, name);
                return;
            }

            Array.Resize(ref localizedNames, localizedNames.Length + 1);
            localizedNames[^1] = new CharacterLocalizedName(language, name);
        }

        public void SetNameForAllLanguages(string name)
        {
            var languages = (Language[])Enum.GetValues(typeof(Language));
            int languageCount = 0;
            foreach (var language in languages)
            {
                if (language != Language.None && language != Language.Undefined)
                    languageCount++;
            }

            localizedNames = new CharacterLocalizedName[languageCount];
            int index = 0;
            foreach (var language in languages)
            {
                if (language == Language.None || language == Language.Undefined)
                    continue;

                localizedNames[index] = new CharacterLocalizedName(language, name);
                index++;
            }
        }

        private string Ctx(string message) => $"[{nameof(CharacterInfoSO)}:{name}] {message}";
    }
}
