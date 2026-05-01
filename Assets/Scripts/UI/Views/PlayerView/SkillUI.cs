using System;
using Actors.PlayerSystem;
using BlackboxSystem;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI.PlayerView
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SkillRouletteManager), typeof(SkillSelectionManager), typeof(SkillCooltimeManager))]
    internal class SkillUI : MonoBehaviour
    {
        [Header("Roulette Result")]
        [SerializeField, Min(0)] private float _rouletteResultFontSize = 72f;
        [SerializeField] private Color _rouletteResultTextColor = new Color(1f, 0.93f, 0.3f, 1f);
        [SerializeField, Range(0, 1)] private float _rouletteResultOverlayAlpha = 0.85f;

        private SkillRouletteManager _skillRouletteManager;
        private SkillSelectionManager _skillSelectionManager;
        private SkillCooltimeManager _skillCooltimeManager;
        private GameObject _rouletteResultOverlay;
        private TextMeshProUGUI _rouletteResultText;

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            _skillRouletteManager = GetComponent<SkillRouletteManager>();   
            _skillSelectionManager = GetComponent<SkillSelectionManager>();
            _skillCooltimeManager = GetComponent<SkillCooltimeManager>();

            BlackboxHandle.Of(this).Exert(_skillSelectionManager, "Awake from SkillUI");
            BlackboxHandle.Of(this).Exert(_skillCooltimeManager, "Awake from SkillUI");
        }

        // Skill Roulette Manager
        public void EnableRoulette(int index, Action callback) => _skillRouletteManager.Enable(index, callback);
        public void DisableRoulette() => _skillRouletteManager.Disable();

        // Skill Selecton Manager
        public void InitializeSkills(SkillType[] skills) => _skillSelectionManager.Initialize(skills);
        public void OnSkillChanged(SkillType targetSkill) => _skillSelectionManager.OnSkillChanged(targetSkill);
        public void AddSkill(SkillType targetSkill) => _skillSelectionManager.AddSkill(targetSkill);

        public void ShowRouletteResult(float bonus)
        {
            EnsureRouletteResultOverlay();

            _rouletteResultText.text = $"x{bonus:0.##}";
            _rouletteResultOverlay.SetActive(true);
            _rouletteResultOverlay.transform.SetAsLastSibling();
        }

        public void ClearRouletteResult()
        {
            if (!_rouletteResultOverlay)
                return;

            _rouletteResultText.text = string.Empty;
            _rouletteResultOverlay.SetActive(false);
        }

        // Skill Cooltime Manager
        public void EnableCooltime(Func<float> getCooltimeRate) => _skillCooltimeManager.Enable(getCooltimeRate);
        public void DisableCooltime() => _skillCooltimeManager.Disable();

        private void EnsureRouletteResultOverlay()
        {
            if (_rouletteResultOverlay)
                return;

            var parentCanvas = GetComponentInParent<Canvas>();
            var parent = parentCanvas ? parentCanvas.transform : transform;

            _rouletteResultOverlay = new GameObject(
                "Roulette Result Overlay",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));

            var overlayTransform = _rouletteResultOverlay.GetComponent<RectTransform>();
            overlayTransform.SetParent(parent, false);
            overlayTransform.anchorMin = Vector2.zero;
            overlayTransform.anchorMax = Vector2.one;
            overlayTransform.offsetMin = Vector2.zero;
            overlayTransform.offsetMax = Vector2.zero;

            var overlayImage = _rouletteResultOverlay.GetComponent<Image>();
            overlayImage.color = new Color(0f, 0f, 0f, _rouletteResultOverlayAlpha);
            overlayImage.raycastTarget = false;

            var textObject = new GameObject(
                "Roulette Result Text",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));

            var textTransform = textObject.GetComponent<RectTransform>();
            textTransform.SetParent(overlayTransform, false);
            textTransform.anchorMin = Vector2.zero;
            textTransform.anchorMax = Vector2.one;
            textTransform.offsetMin = Vector2.zero;
            textTransform.offsetMax = Vector2.zero;

            _rouletteResultText = textObject.GetComponent<TextMeshProUGUI>();
            _rouletteResultText.alignment = TextAlignmentOptions.Center;
            _rouletteResultText.color = _rouletteResultTextColor;
            _rouletteResultText.enableAutoSizing = true;
            _rouletteResultText.fontSizeMin = 24f;
            _rouletteResultText.fontSizeMax = _rouletteResultFontSize;
            _rouletteResultText.fontStyle = FontStyles.Bold;
            _rouletteResultText.raycastTarget = false;

            _rouletteResultOverlay.SetActive(false);
        }

        private void OnDestroy()
        {
            if (_rouletteResultOverlay)
                Destroy(_rouletteResultOverlay);
        }
    }
}
