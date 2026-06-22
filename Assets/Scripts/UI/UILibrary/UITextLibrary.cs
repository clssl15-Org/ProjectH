using System;
using System.Collections.Generic;
using Infrastructure;
using UnityEngine;

namespace UI
{
    public enum UITextResource
    {
        None,

        Title_Play,
        Title_Guide,
        Title_Settings,
        Title_Exit,

        Settings_Language,
        Settings_DisplayLanguage,
        Settings_Settings,
        Settings_Bgm,
        Settings_Sfx,
        Settings_Resume,
        Settings_Restart,
        Settings_Guide,
        Settings_RelicOfTheOther,
        Settings_Exit,

        Coin_ScrollToFlipCoin,
        Coin_SuccessBonus,
        Coin_Failed,
        Coin_Success,

        RelicOfTheOrder_Title,
        Stage0_NamePlaceholder,

        RecordUnlocked_Message1,
        RecordUnlocked_Message2,
    }
    
    public class UITextLibrary : MonoBehaviour
    {
        // Front
        [SerializeField] private TextAsset _textFile;

        // Internal
        private static UITextLibrary _instance;
        private Dictionary<UITextResource, Dictionary<Language, string>> _texts;


        // Content
        private void Awake()
        {
            if (_instance) return;
            _instance = this;

            _texts = Parse(_textFile);
        }

        public static string GetText(UITextResource name, Language language)
        {
            if (_instance == null)
                throw new InvalidOperationException(
                    Ctx("UI 텍스트 라이브러리 인스턴스를 찾을 수 없습니다."));

            if (_instance._texts == null)
                throw new InvalidOperationException(
                    Ctx("UI 텍스트 데이터가 로드되지 않았습니다."));

            if (!_instance._texts.TryGetValue(name, out var textsByLanguage))
                throw new InvalidOperationException(
                    Ctx($"{name} UI 텍스트 패키지를 찾을 수 없습니다."));

            if (!textsByLanguage.TryGetValue(language, out var text))
                throw new InvalidOperationException(
                    Ctx($"{name} 텍스트에서 {language} 언어 문자열을 찾을 수 없습니다."));

            if (string.IsNullOrEmpty(text))
                throw new InvalidOperationException(
                    Ctx($"{name}/{language} 문자열이 할당되지 않았습니다."));

            return text;
        }

        private static Dictionary<UITextResource, Dictionary<Language, string>> Parse(TextAsset textFile)
        {
            if (textFile == null)
                throw new InvalidOperationException(
                    Ctx("UI 텍스트 파일이 할당되지 않았습니다."));

            var texts = new Dictionary<UITextResource, Dictionary<Language, string>>();
            var currentName = UITextResource.None;
            var lines = textFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            for (var i = 0; i < lines.Length; i++)
            {
                var lineNumber = i + 1;
                var line = lines[i].Trim();

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                var separatorIndex = line.IndexOf(':');
                if (separatorIndex < 0)
                {
                    currentName = ParseName(line, lineNumber);
                    if (!texts.ContainsKey(currentName))
                        texts[currentName] = new Dictionary<Language, string>();

                    continue;
                }

                if (currentName == UITextResource.None)
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄의 언어 문자열 앞에 UI 텍스트 이름이 없습니다."));

                var languageCode = line[..separatorIndex].Trim();
                var text = line[(separatorIndex + 1)..].Trim();
                var language = ParseLanguage(languageCode, lineNumber);

                if (texts[currentName].ContainsKey(language))
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄에서 {currentName}/{language} 문자열이 중복되었습니다."));

                texts[currentName][language] = text;
            }

            return texts;
        }

        private static UITextResource ParseName(string value, int lineNumber)
        {
            if (!Enum.TryParse(value, out UITextResource name) || name == UITextResource.None)
                throw new InvalidOperationException(
                    Ctx($"{lineNumber}번째 줄의 UI 텍스트 이름 '{value}'을(를) 해석할 수 없습니다."));

            return name;
        }

        private static Language ParseLanguage(string value, int lineNumber)
        {
            return value.ToLowerInvariant() switch
            {
                "ko" => Language.Korean,
                "en" => Language.Engilsh,
                _ => throw new InvalidOperationException(
                    Ctx($"{lineNumber}번째 줄의 언어 코드 '{value}'을(를) 해석할 수 없습니다."))
            };
        }

        private static string Ctx(string message) => $"[{nameof(UITextLibrary)}] {message}";
    }
}
