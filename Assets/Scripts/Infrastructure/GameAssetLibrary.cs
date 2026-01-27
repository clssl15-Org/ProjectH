using System.Linq;
using TMPro;
using UnityEngine;
using World;

namespace Infrastructure
{
    public class GameAssetLibrary : MonoBehaviour
    {
        [Header("Indicators")]
        [SerializeField] private GameObject _playerDetection;
        [SerializeField] private GameObject _exclamationMark;
        [SerializeField] private TextMeshPro _text;

        [Header("Materials")]
        [SerializeField] private Material _solidColor;

        [Header("Characters / Dialogues")]
        [SerializeField] private CharacterInfoSO[] _characters;
        [SerializeField] private DialogueScriptLibrary _dialogueScriptLibrary;


        public GameObject Indicator_PlayerDetection => Instantiate(_playerDetection);
        public GameObject Indicator_ExclamationMark => Instantiate(_exclamationMark);
        public TextMeshPro Indicator_Text => Instantiate(_text);

        public Material Materials_SolidColor => Instantiate(_solidColor);
        public DialogueScriptLibrary DialogueScriptLibrary => _dialogueScriptLibrary;

        public bool TryGetCharacterInfo(Character character, out CharacterInfoSO characterInfo)
        {
            characterInfo = _characters.FirstOrDefault(c => c.Character == character);
            return characterInfo != null;
        }
    }
}
