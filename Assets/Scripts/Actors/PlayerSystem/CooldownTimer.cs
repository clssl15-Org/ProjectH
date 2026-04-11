using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Actors.PlayerSystem
{
    public class CooldownTimer : MonoBehaviour, ISkillCoolDownTimer
    {
        public bool IsOnCooldown
        {
            get => timeRemaining > 0;
        }
        public float TimeRemaining
        {
            get => timeRemaining;
            set => timeRemaining = value;
        }
        public float Progress
        {
            get => (totalTime > 0) ? timeRemaining / totalTime : 0f;
        }
        public CooldownType CooldownType { get; set; }

        public event Action OnCooldownStart;
        public event Action OnCooldownComplete;

        private float totalTime;
        private float timeRemaining;

        public void StartCooldown(float duration, float dt)
        {
            totalTime = duration;
            timeRemaining = duration;

            StartCoroutine(CooldownCoroutine(dt));

            OnCooldownStart?.Invoke();
        }

        IEnumerator CooldownCoroutine(float dt)
        {
            while (timeRemaining > 0)
            {
                timeRemaining -= dt;
                yield return new WaitForSeconds(dt);
            }

            timeRemaining = 0;

            OnCooldownComplete?.Invoke();

            Destroy(this);
        }


    }
}
