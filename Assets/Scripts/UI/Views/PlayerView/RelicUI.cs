using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerView
{
    [RequireComponent(typeof(Image))]
    internal class RelicUI : MonoBehaviour
    {
        public int ID { get; private set; }
        private Image _img;

        private void Awake() =>
            _img = GetComponent<Image>();

        public void Initialize(RelicDataSO relicData)
        {
            ID = relicData.RelicNumber;
            _img.sprite = relicData.Icon;
        }
    }
}
