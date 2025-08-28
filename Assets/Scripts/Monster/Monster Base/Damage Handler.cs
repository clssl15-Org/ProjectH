using System.Collections;
using UnityEngine;

internal class DamageHandler
{
    // Internal
    private readonly Monster owner;
    private readonly float damageAnimationLength;
    private readonly float invincibleTime;
    private bool isDamaging = false;


    // Content
    public DamageHandler(Monster owner, float damageAnimationLength, float invincibleTime)
    {
        this.owner = owner;
        this.damageAnimationLength = damageAnimationLength;
        this.invincibleTime = invincibleTime;
    }

    public bool TryTakeDamage(int damage, out IEnumerator routine)
    {
        if (isDamaging)
        {
            routine = null;
            return false;
        }

        isDamaging = true;
        owner.HP -= damage;

        var remainingTime = invincibleTime;
        var materialRestored = false;

        var originalMaterial = owner.SpriteRenderer.material;
        owner.SpriteRenderer.material = owner.sceneAssetsLibrary.SolidColor;
        owner.SpriteRenderer.material.color = Color.white;

        IEnumerator DoTakeDamage()
        {
            while (remainingTime > 0)
            {
                yield return null;
                if (!owner.SpriteRenderer) yield break;

                remainingTime -= Time.deltaTime;

                if (!materialRestored && remainingTime <= invincibleTime - damageAnimationLength)
                {
                    materialRestored = true;
                    owner.SpriteRenderer.material = originalMaterial;
                }

            };

            if (!materialRestored && owner.SpriteRenderer)
                owner.SpriteRenderer.material = originalMaterial;

            isDamaging = false;
        }

        routine = DoTakeDamage();
        return true;
    }
}
