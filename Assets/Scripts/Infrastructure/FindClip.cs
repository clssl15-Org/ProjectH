using System;
using System.Collections.Generic;
using UnityEngine;

public static partial class AnimatorExtensions
{
    public static bool TryFindClip(this Animator animator, string clipName, out AnimationClip clip)
    {
        clip = animator.FindClip(clipName);

        if (clip == null) clipName = clipName.ToLower();
        clip = animator.FindClip(clipName);
        
        return clip != null;
    }

    public static AnimationClip FindClip(this Animator animator, string clipName, StringComparison stringComparison = StringComparison.Ordinal)
    {
        if (!animator) throw new ArgumentNullException(nameof(animator));
        var rac = animator.runtimeAnimatorController;
        if (!rac || string.IsNullOrEmpty(clipName)) return null;

        if (rac is AnimatorOverrideController aoc)
        {
            var list = new List<KeyValuePair<AnimationClip, AnimationClip>>(aoc.overridesCount);
            aoc.GetOverrides(list);
            foreach (var kv in list)
            {
                var candidate = kv.Value != null ? kv.Value : kv.Key;
                if (candidate && string.Equals(candidate.name, clipName, stringComparison))
                    return candidate;
            }
        }

        foreach (var clip in rac.animationClips)
        {
            if (clip && string.Equals(clip.name, clipName, stringComparison))
                return clip;
        }

        return null;
    }
}
