using System.Collections;
using System.Collections.Generic;
using Actors.PlayerSystem;
using UnityEngine;

public class DamageRoulette : MonoBehaviour
{
    public bool isApplied = false;

    private SkillManager skillManager;

    // 룰렛 보너스 수치 정의
    private readonly float[] bonuses = { 1, 1.1f, 1.25f, 1.5f, 1.75f, 2 };
    // 기획서에 명시된 각 보너스별 확률 (%)
    private readonly int[] probabilities = { 22, 30, 25, 15, 6, 2 };

    private void Awake()
    {
        skillManager = this.transform.root.GetComponentInChildren<SkillManager>();
    }
    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") == 0) return;

        if (isApplied) return;

        isApplied = true;
        float currentBonus = ApplyRoulette();
        this.transform.root.GetComponentInChildren<Player>().RouletteDamageMultiplier = currentBonus;
        skillManager.canChangeSkill = false;

    }

    private float ApplyRoulette()
    {
        int randomValue = Random.Range(0, 100);
        int cumulative = 0;

        for (int i = 0; i < probabilities.Length; i++)
        {
            cumulative += probabilities[i];
            if (randomValue < cumulative)
            {
                print($"{bonuses[i]} is selected!");
                return bonuses[i];
            }
        }

        return 1; // 기본값
    }

    // 스킬 사용 후 호출하여 룰렛 상태를 리셋하는 함수
    public void ResetRoulette()
    {
        isApplied = false;
        skillManager.canChangeSkill = true;
    }
}
