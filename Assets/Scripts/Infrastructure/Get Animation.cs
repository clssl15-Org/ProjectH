using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class Tools
{
    /// <summary>
    /// Animator(및 AnimatorOverrideController 포함)에서 이름으로 AnimationClip을 찾습니다.
    /// </summary>
    public static AnimationClip FindClip(this Animator animator, string clipName, StringComparison stringComparison = StringComparison.Ordinal)
    {
        if (!animator) throw new ArgumentNullException(nameof(animator));
        var rac = animator.runtimeAnimatorController;
        if (!rac || string.IsNullOrEmpty(clipName)) return null;

        // 1) 오버라이드 컨트롤러면, 실제로 적용된 클립(override 우선)을 먼저 훑습니다.
        if (rac is AnimatorOverrideController aoc)
        {
            var list = new List<KeyValuePair<AnimationClip, AnimationClip>>(aoc.overridesCount);
            aoc.GetOverrides(list); // 원본->오버라이드 매핑 조회 (할당 방지: 미리 용량 지정) :contentReference[oaicite:1]{index=1}
            foreach (var kv in list)
            {
                var candidate = kv.Value != null ? kv.Value : kv.Key; // 오버라이드가 없으면 원본
                if (candidate && string.Equals(candidate.name, clipName, stringComparison))
                    return candidate;
            }
        }

        // 2) 일반 컨트롤러(또는 보조 탐색): 이 컨트롤러가 사용하는 모든 클립 열거
        foreach (var clip in rac.animationClips) // 컨트롤러가 사용하는 모든 AnimationClip 반환 :contentReference[oaicite:2]{index=2}
        {
            if (clip && string.Equals(clip.name, clipName, stringComparison))
                return clip;
        }

        return null;
    }
}
