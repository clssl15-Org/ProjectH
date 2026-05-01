using System;
using Actors.PlayerSystem;
using BlackboxSystem;
using TMPro;
using UnityEngine;

namespace UI.PlayerView
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(SkillRouletteManager), typeof(SkillSelectionManager), typeof(SkillCooltimeManager))]
    internal class SkillUI : MonoBehaviour
    {
        [Header("Roulette Result")]
        [SerializeField, Min(0)] private float _rouletteResultFontSize = 54f;
        [SerializeField] private Color _rouletteResultTextColor = new Color(1f, 0.93f, 0.3f, 1f);

        private SkillRouletteManager _skillRouletteManager;
        private SkillSelectionManager _skillSelectionManager;
        private SkillCooltimeManager _skillCooltimeManager;
        private TextMeshProUGUI _rouletteResultText;

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).WriteScope("Awake");

            _skillRouletteManager = GetComponent<SkillRouletteManager>();   
            _skillSelectionManager = GetComponent<SkillSelectionManager>();
            _skillCooltimeManager = GetComponent<SkillCooltimeManager>();

            BlackboxHandle.Of(this).Exert(_skillSelectionManager, "Awake from SkillUI");
            BlackboxHandle.Of(this).Exert(_skillCooltimeManager, "Awake from SkillUI");

            EnsureRouletteResultText();
            ClearRouletteResult();
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
            EnsureRouletteResultText();

            _rouletteResultText.text = $"x{bonus:0.##}";
            _rouletteResultText.gameObject.SetActive(true);
            _rouletteResultText.transform.SetAsLastSibling();
        }

        public void ClearRouletteResult()
        {
            EnsureRouletteResultText();

            _rouletteResultText.text = string.Empty;
            _rouletteResultText.gameObject.SetActive(false);
        }

        // Skill Cooltime Manager
        public void EnableCooltime(Func<float> getCooltimeRate) => _skillCooltimeManager.Enable(getCooltimeRate);
        public void DisableCooltime() => _skillCooltimeManager.Disable();

        private void EnsureRouletteResultText()
        {
            if (_rouletteResultText)
                return;

            var textObject = new GameObject(
                "Roulette Result",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));

            var rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.SetParent(transform, false);
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            _rouletteResultText = textObject.GetComponent<TextMeshProUGUI>();
            _rouletteResultText.alignment = TextAlignmentOptions.Center;
            _rouletteResultText.color = _rouletteResultTextColor;
            _rouletteResultText.enableAutoSizing = true;
            _rouletteResultText.fontSizeMin = 24f;
            _rouletteResultText.fontSizeMax = _rouletteResultFontSize;
            _rouletteResultText.fontStyle = FontStyles.Bold;
            _rouletteResultText.raycastTarget = false;
        }
    }
}
