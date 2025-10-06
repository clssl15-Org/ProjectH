using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.iOS;

public class CooldownTiemr : MonoBehaviour
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

    private float totalTime;
    private float timeRemaining;

    public void StartCooldown(float duration, float dt)
    {
        totalTime = duration;
        timeRemaining = duration;

        StartCoroutine(CooldownCoroutine(dt));
    }
    
    IEnumerator CooldownCoroutine(float dt)
    {
        while (timeRemaining > 0)
        {
            timeRemaining -= dt;
            yield return new WaitForSeconds(dt);
        }

        timeRemaining = 0;
        Destroy(this);
    }


}
