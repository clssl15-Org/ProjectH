using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Actors.PlayerSystem;
using UnityEngine;

public class DamageRoulette : MonoBehaviour
{
    public bool isApplied = false;

    private SkillManager skillManager;

    private Player player;

    // 룰렛 보너스 수치 정의
    private readonly float[] bonuses = { 1, 1.1f, 1.25f, 1.5f, 1.75f, 2 };
    // 기획서에 명시된 각 보너스별 확률 (%)
    private int[] probabilities = { 22, 30, 25, 15, 6, 2 };
    public int[] Probabilities
    {
        get => probabilities;
        set
        {
            probabilities = value;
        }
    }

    // --------
    public class Context
    {
        public float[] Bonuses { get; }
        public int[] Probabilities { get; }

        private readonly Action<float> _applied;

        public Context(float[] bonuses, int[] probabilities, Action<float> applied)
        {
            Bonuses = bonuses;
            Probabilities = probabilities;
            _applied = applied;
        }

        public void Apply(float bonus) => _applied?.Invoke(bonus);
    }

    private object _currentRouletteToken;
    // --------

    private void Awake()
    {
        skillManager = this.transform.root.GetComponentInChildren<SkillManager>();
        player = this.transform.root.GetComponentInChildren<Player>();
    }
    private void Update()
    {
        //if (Input.GetAxis("Mouse ScrollWheel") == 0) return;

        //if (isApplied) return;

        //isApplied = true;
        //float currentBonus = ApplyRoulette();
        //this.transform.root.GetComponentInChildren<Player>().RouletteDamageMultiplier = currentBonus;
        //skillManager.canChangeSkill = false;
    }

    public bool TrySkillRoulette(out Context context)
    {
        context = default;

        if (skillManager.skills.Count <= 0) return false;
        if (player.CurrentSkillCooldown > 0f) return false;
        if (isApplied) return false;

        isApplied = true;

        var token = _currentRouletteToken = new();
        context = new Context(bonuses.ToArray(), probabilities.ToArray(), bonus =>
        {
            if (token != _currentRouletteToken)
                return;

            transform.root.GetComponentInChildren<Player>().RouletteDamageMultiplier = bonus;
            skillManager.canChangeSkill = false;
        });

        return true;
    }

    //private float ApplyRoulette()
    //{
    //    int randomValue = UnityEngine.Random.Range(0, 100);
    //    int cumulative = 0;

    //    for (int i = 0; i < probabilities.Length; i++)
    //    {
    //        cumulative += probabilities[i];
    //        if (randomValue < cumulative)
    //        {
    //            print($"{bonuses[i]} is selected!");
    //            return bonuses[i];
    //        }
    //    }

    //    return 1; // 기본값
    //}

    // 스킬 사용 후 호출하여 룰렛 상태를 리셋하는 함수
    public void ResetRoulette()
    {
        _currentRouletteToken = null;

        isApplied = false;
        skillManager.canChangeSkill = true;
    }
}
