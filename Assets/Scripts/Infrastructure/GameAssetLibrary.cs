using TMPro;
using UnityEngine;

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


        public GameObject Indicator_PlayerDetection => Instantiate(_playerDetection);
        public GameObject Indicator_ExclamationMark => Instantiate(_exclamationMark);
        public TextMeshPro Indicator_Text => Instantiate(_text);

        public Material SolidColor => Instantiate(_solidColor);
    }
}
