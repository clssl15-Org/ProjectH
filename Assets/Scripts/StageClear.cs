using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StageClear : MonoBehaviour
{
    public GameObject monstersParent;      // Monsters 오브젝트
    public GameObject objectToActivate;    // 활성화할 오브젝트

    void Update()
    {
        if (AllChildrenInactive(monstersParent))
        {
            objectToActivate.SetActive(true);
            enabled = false;  // 조건 만족 후 더 이상 검사 안 함 
        }
    }

    bool AllChildrenInactive(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            if (child.gameObject.activeInHierarchy)
            {
                return false;  // 하나라도 활성화된 게 있으면 false
            }
        }
        return true;  // 전부 비활성화됨
    }
}
