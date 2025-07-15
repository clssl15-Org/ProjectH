using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GainQGaugeOnAttack : MonoBehaviour
{
    [SerializeField] PlayerQSkill playerQSkill;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            playerQSkill.IncreaseQSkillGauge();
        }
    }
}
