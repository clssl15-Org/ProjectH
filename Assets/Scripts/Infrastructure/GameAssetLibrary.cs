using System.Linq;
using TMPro;
using UnityEngine;
using World;

namespace Infrastructure
{
    public class GameAssetLibrary : MonoBehaviour
    {
        public static Color DefaultUiBlueHighlightColor { get; } = new Color(0.2f, 0.5411765f, 0.8313726f, 1f);
        public static Color DefaultUiRedHighlightColor { get; } = new Color(1f, 0f, 0f, 1f);

        [Header("Indicators")]
        [SerializeField] private GameObject _playerDetection;
        [SerializeField] private GameObject _exclamationMark;
        [SerializeField] private TextMeshPro _text;

        [Header("Materials")]
        [SerializeField] private Material _solidColor;

        [Header("UI Colors")]
        [SerializeField] private Color _uiBlueHighlightColor = new Color(0.2f, 0.5411765f, 0.8313726f, 1f);
        [SerializeField] private Color _uiRedHighlightColor = new Color(1f, 0f, 0f, 1f);

        [Header("Characters / Dialogues")]
        [SerializeField] private CharacterInfoSO[] _characters;


        public GameObject Indicator_PlayerDetection => Instantiate(_playerDetection);
        public GameObject Indicator_ExclamationMark => Instantiate(_exclamationMark);
        public TextMeshPro Indicator_Text => Instantiate(_text);

        public Material Materials_SolidColor => Instantiate(_solidColor);
        public Color UiBlueHighlightColor => _uiBlueHighlightColor;
        public Color UiRedHighlightColor => _uiRedHighlightColor;

        public bool TryGetCharacterInfo(Character character, out CharacterInfoSO characterInfo)
        {
            characterInfo = _characters.FirstOrDefault(c => c.Character == character);
            return characterInfo != null;
        }
    }
}
