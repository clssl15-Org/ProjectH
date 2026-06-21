using Game.Management;
using UnityEngine;

namespace UI
{
    public class LanguageSelectorButton : MonoBehaviour
    {
        [SerializeField] private bool _toNext;

        public void SetNextLanguage() =>
            LanguageManager.SetNextLanguage(_toNext);
    }
}
