
using UnityEngine;

public class PlayerManger : MonoBehaviour
{
    private static PlayerManger _instance = null;   // 싱글통

    // 플레이어 상태 가져오기
    public PlayerState playerState;

    // HP 관련
    public float maxHP = 100f;
    public float currentHP = 100f;

    // Q 게이지 관련  - PlayerQSkill에서 값을 업데이트 해줌
    public float maxQSkillGauge;
    public float currentQSkillGauge;

    // UI 참조 (필요 시)
    // [SerializeField] private UIHealthBar healthBar; // HP UI
    // [SerializeField] private UIQGauge qGauge; // Q 게이지 UI

    public static PlayerManger Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<PlayerManger>();
                if (_instance == null)
                {
                    GameObject singletonObject = new GameObject(typeof(PlayerManger).Name);
                    _instance = singletonObject.AddComponent<PlayerManger>();
                    // DontDestroyOnLoad(singletonObject); // 씬이 바뀌어도 유지되도록 설정 (선택 사항)
                }
            }
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        // DontDestroyOnLoad(gameObject); // 씬이 바뀌어도 유지되도록 설정 (선택 사항)

        playerState = PlayerState.Instance;
    }


    // 플레이어 hp 관련
    public void IncreaseCurrentHP(float HP)
    {     // hp 증가
        if (currentHP < maxHP)
        {    // hp가 다 안 찼으면
            currentHP += HP;        // currentHP에 HP 더해주기
            currentHP = currentHP > maxHP ? maxHP : currentHP;      // Max 보다 크면 Maxhp 로
        }
    }

    public void DecreaseCurrentHP(float HP)
    {     // hp 감소
        if (currentHP > 0)
        {    // hp가 0보다 크면
            currentHP -= HP;        // currentHP에 HP 빼주기
            currentHP = currentHP < 0 ? 0 : currentHP;      // 0 보다 작면 0으로
        } 
        if (currentHP <= 0) {   // currentHP이 0보다 작으면
            playerState.ChangeState(PlayerState.State.Dead);
            playerState.isDead = true;
        }
    }

}
