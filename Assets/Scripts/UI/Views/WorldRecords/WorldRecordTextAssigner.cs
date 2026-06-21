using System;
using System.Collections.Generic;
using System.Text;
using Game.Management;
using Infrastructure;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class WorldRecordTextAssigner : MonoBehaviour
    {
        [SerializeField] private TextAsset _textFile;

        private TextMeshProUGUI _text;
        private Dictionary<Language, string> _texts;


        private void Awake()
        {
            _text = GetComponent<TextMeshProUGUI>();
            _texts = Parse(_textFile);
            LanguageManager.LanguageChanged += AssignText;
        }

        private void Start()
        {
            AssignText(LanguageManager.Language);
        }

        private void AssignText(Language language)
        {
            if (!this) return;

            if (!_texts.TryGetValue(language, out var text))
                throw new InvalidOperationException(
                    Ctx($"{language} 월드 레코드 텍스트를 찾을 수 없습니다."));

            _text.text = text;
        }

        private static Dictionary<Language, string> Parse(TextAsset textFile)
        {
            if (textFile == null)
                throw new InvalidOperationException(
                    Ctx("월드 레코드 텍스트 파일이 할당되지 않았습니다."));

            var texts = new Dictionary<Language, string>();
            var builder = new StringBuilder();
            var currentLanguage = Language.None;
            var lines = textFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

            foreach (var line in lines)
            {
                if (TryReadHeader(line, out var language))
                {
                    Flush(texts, currentLanguage, builder);
                    currentLanguage = language;
                    continue;
                }

                if (currentLanguage == Language.None)
                {
                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    throw new InvalidOperationException(
                        Ctx("언어 헤더 앞에 월드 레코드 본문이 있습니다."));
                }

                builder.AppendLine(line);
            }

            Flush(texts, currentLanguage, builder);
            return texts;
        }

        private static bool TryReadHeader(string line, out Language language)
        {
            language = line.Trim().ToLowerInvariant() switch
            {
                "ko:" => Language.Korean,
                "en:" => Language.Engilsh,
                _ => Language.None
            };

            return language != Language.None;
        }

        private static void Flush(
            Dictionary<Language, string> texts,
            Language language,
            StringBuilder builder)
        {
            if (language == Language.None)
                return;

            if (texts.ContainsKey(language))
                throw new InvalidOperationException(
                    Ctx($"{language} 월드 레코드 텍스트가 중복되었습니다."));

            var text = builder.ToString().TrimEnd('\r', '\n');
            if (string.IsNullOrWhiteSpace(text))
                throw new InvalidOperationException(
                    Ctx($"{language} 월드 레코드 텍스트가 비어 있습니다."));

            texts.Add(language, text);
            builder.Clear();
        }

        private void OnDestroy()
        {
            LanguageManager.LanguageChanged -= AssignText;
        }

        private static string Ctx(string message) => $"[{nameof(WorldRecordTextAssigner)}] {message}";
    }
}
