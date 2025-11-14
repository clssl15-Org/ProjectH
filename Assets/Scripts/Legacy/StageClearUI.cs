using UnityEngine;
using UnityEngine.UI;

public class StageClearUI : MonoBehaviour
{
    public GameObject panel; // 흐린 배경
    public GameObject stageClearImage; // 스테이지 클리어 이미지

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            ShowStageClear();
        }
    }

    void ShowStageClear()
    {
        panel.SetActive(true);
        stageClearImage.SetActive(true);

        // 게임 정지
        Time.timeScale = 0f;
    }
}
