using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Game.Management;
using Infrastructure;
using TMPro;
using UnityEngine;
using World;

[RequireComponent(typeof(TextMeshProUGUI))]
public class EndingTextAssigner : MonoBehaviour, IInjectable<GameAssetLibrary>
{
    [SerializeField] private TextAsset _textFile;
    [SerializeField] private string _resourceName = "Ending_Main";

    private TextMeshProUGUI _text;
    private Dictionary<string, Dictionary<Language, string>> _texts;
    private GameAssetLibrary _gameAssetLibrary;

    private void Awake()
    {
        _text = GetComponent<TextMeshProUGUI>();
        _texts = Parse(_textFile);
    }

    void IInjectable<GameAssetLibrary>.Inject(GameAssetLibrary gameAssetLibrary)
        => _gameAssetLibrary = gameAssetLibrary;

    private void Start()
    {
        LanguageManager.LanguageChanged += AssignText;
        AssignText(LanguageManager.Language);
    }

    private void AssignText(Language language)
    {
        if (!this) return;

        if (!_texts.TryGetValue(_resourceName, out var textsByLanguage))
            throw new InvalidOperationException(
                Ctx($"{_resourceName} 엔딩 텍스트를 찾을 수 없습니다."));

        if (!textsByLanguage.TryGetValue(language, out var text))
            throw new InvalidOperationException(
                Ctx($"{_resourceName}/{language} 엔딩 텍스트를 찾을 수 없습니다."));

        _text.text = ReplacePlayerName(text, language);
    }

    private string ReplacePlayerName(string text, Language language)
    {
        if (!text.Contains("{player}", StringComparison.OrdinalIgnoreCase))
            return text;

        if (_gameAssetLibrary == null)
            throw new InvalidOperationException(
                Ctx($"{nameof(GameAssetLibrary)}가 주입되지 않아 플레이어 이름을 치환할 수 없습니다."));

        if (!_gameAssetLibrary.TryGetCharacterInfo(Character.Player, out var playerInfo))
            throw new InvalidOperationException(
                Ctx($"{nameof(GameAssetLibrary)}에서 {Character.Player} 정보를 찾을 수 없습니다."));

        return text.Replace(
            "{player}",
            playerInfo.GetName(language),
            StringComparison.OrdinalIgnoreCase);
    }

    private static Dictionary<string, Dictionary<Language, string>> Parse(TextAsset textFile)
    {
        if (textFile == null)
            throw new InvalidOperationException(
                Ctx("엔딩 텍스트 파일이 할당되지 않았습니다."));

        var texts = new Dictionary<string, Dictionary<Language, string>>();
        var currentName = string.Empty;
        var currentLanguage = Language.None;
        var builder = new StringBuilder();
        var lines = textFile.text.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        for (var i = 0; i < lines.Length; i++)
        {
            var lineNumber = i + 1;
            var rawLine = lines[i];
            var line = rawLine.Trim();

            if (currentLanguage != Language.None)
            {
                if (TryReadResourceOpeningTag(line, out _) || TryReadResourceClosingTag(line))
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄에 언어 블록 안의 리소스 태그가 있습니다."));

                if (TryReadClosingTag(line, out var closingLanguage))
                {
                    if (closingLanguage != currentLanguage)
                        throw new InvalidOperationException(
                            Ctx($"{lineNumber}번째 줄의 닫는 태그가 현재 언어 블록과 다릅니다."));

                    Flush(texts, currentName, currentLanguage, builder, lineNumber);
                    currentLanguage = Language.None;
                    continue;
                }

                if (TryReadOpeningTag(line, out _))
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄에 중첩된 언어 태그가 있습니다."));

                builder.AppendLine(rawLine);
                continue;
            }

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            if (TryReadResourceOpeningTag(line, out var resourceName))
            {
                if (!string.IsNullOrWhiteSpace(currentName))
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄에 중첩된 리소스 태그가 있습니다."));

                if (texts.ContainsKey(resourceName))
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄에서 {resourceName} 리소스가 중복되었습니다."));

                currentName = resourceName;
                texts[currentName] = new Dictionary<Language, string>();

                continue;
            }

            if (TryReadResourceClosingTag(line))
            {
                if (string.IsNullOrWhiteSpace(currentName))
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄에 여는 태그 없는 리소스 닫는 태그가 있습니다."));

                if (texts[currentName].Count == 0)
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄에서 {currentName} 리소스에 언어 텍스트가 없습니다."));

                currentName = string.Empty;
                continue;
            }

            if (TryReadOpeningTag(line, out var language))
            {
                if (string.IsNullOrWhiteSpace(currentName))
                    throw new InvalidOperationException(
                        Ctx($"{lineNumber}번째 줄의 언어 태그 앞에 엔딩 텍스트 이름이 없습니다."));

                currentLanguage = language;
                builder.Clear();
                continue;
            }

            if (TryReadClosingTag(line, out _))
                throw new InvalidOperationException(
                    Ctx($"{lineNumber}번째 줄에 여는 태그 없는 닫는 태그가 있습니다."));

            throw new InvalidOperationException(
                Ctx($"{lineNumber}번째 줄을 해석할 수 없습니다. 리소스는 <resource name=\"...\"> 태그로 선언해야 합니다."));
        }

        if (currentLanguage != Language.None)
            throw new InvalidOperationException(
                Ctx($"{currentName}/{currentLanguage} 언어 블록이 닫히지 않았습니다."));

        if (!string.IsNullOrWhiteSpace(currentName))
            throw new InvalidOperationException(
                Ctx($"{currentName} 리소스 블록이 닫히지 않았습니다."));

        return texts;
    }

    private static bool TryReadResourceOpeningTag(string line, out string resourceName)
    {
        var match = Regex.Match(
            line,
            "^<resource\\s+name=\"(?<name>[^\"]+)\">$",
            RegexOptions.IgnoreCase);

        resourceName = match.Success ? match.Groups["name"].Value.Trim() : string.Empty;
        if (match.Success && string.IsNullOrWhiteSpace(resourceName))
            throw new InvalidOperationException(
                Ctx("리소스 이름이 비어 있습니다."));

        return match.Success;
    }

    private static bool TryReadResourceClosingTag(string line)
    {
        return string.Equals(line, "</resource>", StringComparison.OrdinalIgnoreCase);
    }

    private static bool TryReadOpeningTag(string line, out Language language)
    {
        language = line.ToLowerInvariant() switch
        {
            "<ko>" => Language.Korean,
            "<en>" => Language.Engilsh,
            _ => Language.None
        };

        return language != Language.None;
    }

    private static bool TryReadClosingTag(string line, out Language language)
    {
        language = line.ToLowerInvariant() switch
        {
            "</ko>" => Language.Korean,
            "</en>" => Language.Engilsh,
            _ => Language.None
        };

        return language != Language.None;
    }

    private static void Flush(
        Dictionary<string, Dictionary<Language, string>> texts,
        string name,
        Language language,
        StringBuilder builder,
        int lineNumber)
    {
        if (!texts.TryGetValue(name, out var textsByLanguage))
        {
            textsByLanguage = new Dictionary<Language, string>();
            texts[name] = textsByLanguage;
        }

        if (textsByLanguage.ContainsKey(language))
            throw new InvalidOperationException(
                Ctx($"{lineNumber}번째 줄에서 {name}/{language} 문자열이 중복되었습니다."));

        var text = builder.ToString().TrimEnd('\r', '\n');
        if (string.IsNullOrWhiteSpace(text))
            throw new InvalidOperationException(
                Ctx($"{lineNumber}번째 줄의 {name}/{language} 문자열이 비어 있습니다."));

        textsByLanguage[language] = text;
        builder.Clear();
    }

    private void OnDestroy()
    {
        LanguageManager.LanguageChanged -= AssignText;
    }

    private static string Ctx(string message) => $"[{nameof(EndingTextAssigner)}] {message}";
}
