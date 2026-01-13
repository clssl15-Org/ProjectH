using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.RelicInfoPanelView
{
    public class RelicInfoUI : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TextMeshProUGUI _name;
        [SerializeField] private TextMeshProUGUI _descriptionTmp;

        public void Initialize(Sprite icon, string name, string description)
        {
            _icon.sprite = icon;
            _name.text = name;
            _descriptionTmp.text = description;
        }
    }
}
