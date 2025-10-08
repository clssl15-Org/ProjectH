using UnityEngine;

public static partial class AnimatorExtensions
{
    public static bool HasParameter(this Animator animator, string paramName)
    {
        foreach (var param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }

        return false;
    }
}
