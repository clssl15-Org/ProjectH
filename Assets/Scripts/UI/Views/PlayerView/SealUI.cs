using TMPro;
using UnityEngine;

namespace UI.PlayerView
{
    internal class SealUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _tmp;

        public void SetCount(int count) => _tmp.text = $"+{count}";
    }
}
