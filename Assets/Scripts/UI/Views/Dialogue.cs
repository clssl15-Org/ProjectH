using System;
using System.Linq;
using Infrastructure;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(Animation))]
    public class Dialogue : MonoBehaviour, IEnablable
    {
        [SerializeField] private Image _portraitUI;
        [SerializeField] private TextMeshProUGUI _nametagUI;
        [SerializeField] private TextMeshProUGUI _dialogueUI;

        [Serializable]
        public struct PortraitInfo
        {
            public Character Character;
            public Sprite Icon;
        }
        [Space]
        [SerializeField] private PortraitInfo[] _portraits;


        public event Action Disabling;

        #region Interfaces
        Action IEnablable.Enabling => null;
        Action IEnablable.Enabled => null;
        Action IEnablable.Disabling => Disabling;
        Action IEnablable.Disabled => null;
        #endregion

        private EnableWithAnimation _enabler;
        private bool _awaked = false;

        public void Awake()
        {
            if (_awaked) return;
            _awaked = true;

            _enabler = new EnableWithAnimation(GetComponent<Animation>())
                .InitializeWithIEnablable(this);

            SetToDisabled();
        }

        public void SetContent(DialogueData dialogue)
        {
            _portraitUI.sprite = _portraits
                .FirstOrDefault(pi => pi.Character == dialogue.Character)
                .Icon ?? null;

            _nametagUI.text = dialogue.Name;
            _dialogueUI.text = dialogue.Dialogue;
        }


        public void Enable()
        {
            Awake();
            _enabler.Enable();
        }
        public void Disable()
        {
            Awake();
            _enabler.Disable();
        }
        public void SetToEnabled()
        {
            Awake();
            _enabler.SetToEnabled();
        }
        public void SetToDisabled()
        {
            Awake();
            _enabler.SetToDisabled();
        }
    }
}
