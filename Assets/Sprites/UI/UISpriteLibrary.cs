using System;
using Infrastructure;
using UnityEngine;

namespace UI
{
    public enum UISpriteResource
    {
        None,
        Title_TitleText,
        RelicsAndCoins_CheckButton01,
        RelicsAndCoins_CoinButton,
        Others_BackButton00,
        Others_Guide,
        Others_WorldRecords,
        Others_GuideWorldRecords,
    }

    public class UISpriteLibrary : MonoBehaviour
    {
        // Front
        [Serializable]
        public struct SpritePackage
        {
            public UISpriteResource Name;
            public Item[] Items;

            [Serializable]
            public struct Item
            {
                public Language Language;
                public Sprite Sprite;
            }
        }

        [SerializeField] private SpritePackage[] _packages;

        // Internal
        private static UISpriteLibrary _instance;


        // Content
        private void Awake()
        {
            if (_instance) return;
            _instance = this;
        }

        public static Sprite GetSprite(UISpriteResource name, Language language)
        {
            if (_instance == null)
                throw new InvalidOperationException(
                    Ctx("UI 리소스 라이브러리 인스턴스를 찾을 수 없습니다."));

            if (_instance._packages == null)
                throw new InvalidOperationException(
                    Ctx("UI 리소스 패키지 배열이 할당되지 않았습니다."));

            foreach (var package in _instance._packages)
            {
                if (package.Name != name)
                    continue;

                if (package.Items == null)
                    throw new InvalidOperationException(
                        Ctx($"{name} 리소스의 언어별 스프라이트 배열이 할당되지 않았습니다."));

                foreach (var item in package.Items)
                {
                    if (item.Language != language)
                        continue;

                    if (item.Sprite == null)
                        throw new InvalidOperationException(
                            Ctx($"{name}/{language} 스프라이트가 할당되지 않았습니다."));

                    return item.Sprite;
                }

                throw new InvalidOperationException(
                    Ctx($"{name} 리소스에서 {language} 언어 스프라이트를 찾을 수 없습니다."));
            }

            throw new InvalidOperationException(
                Ctx($"{name} UI 리소스 패키지를 찾을 수 없습니다."));
        }

        private static string Ctx(string message) => $"[{nameof(UISpriteLibrary)}] {message}";
    }
}
