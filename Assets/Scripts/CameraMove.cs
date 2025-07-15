using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{
    public Transform targetPos; // 목표 위치 (TargetPos)
    public float speed = 2f;    // 이동 속도
    public GameObject uiButtons; // 버튼 UI

    private bool reached = false;

    void Start()
    {
        uiButtons.SetActive(false); // 버튼 숨기기
    }

    void Update()
    {
        if (!reached)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos.position, speed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetPos.position) < 0.01f)
            {
                reached = true;
                uiButtons.SetActive(true); // 도착하면 버튼 보이기
            }
        }
    }
}
