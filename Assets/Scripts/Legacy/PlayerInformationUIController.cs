using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 사용하는 경우

public class PlayerInformationUIController : MonoBehaviour
{
    [SerializeField] private Slider healthBar;
    [SerializeField] private Image healthBarFill;      // hp 바 채우는 UI
    [SerializeField] private Slider qGauge;
    [SerializeField] private Image qGaugeFill;      // qGauge 바 채우는 UI

    private PlayerManger playerManager;

    private void Start()
    {
        playerManager = PlayerManger.Instance;
        if (playerManager == null)
        {
            Debug.LogError("PlayerManger Instance가 없습니다!");
        }
    }

    private void Update()
    {
        if (playerManager != null)
        {
            UpdateHealthUI();
            UpdateQGaugeUI();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthBar != null)
        {
            healthBar.value = playerManager.currentHP / playerManager.maxHP;

            if (healthBar.value <= 0)
                healthBarFill.color = new Color(healthBarFill.color.r, healthBarFill.color.g, healthBarFill.color.b, 0);
            else
                healthBarFill.color = new Color(healthBarFill.color.r, healthBarFill.color.g, healthBarFill.color.b, 1f);
        }
    }

    private void UpdateQGaugeUI()
    {
        if (qGauge != null)
        {
            qGauge.value = playerManager.currentQSkillGauge / playerManager.maxQSkillGauge;

            if (qGauge.value <= 0)
                qGaugeFill.color = new Color(qGaugeFill.color.r, qGaugeFill.color.g, qGaugeFill.color.b, 0);
            else
                qGaugeFill.color = new Color(qGaugeFill.color.r, qGaugeFill.color.g, qGaugeFill.color.b, 1f);
        }
    }
}