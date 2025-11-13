using System.Collections;
using UnityEngine;

namespace Actors.Monsters
{
    public class CrowProjectile : KinematicProjectile
    {
        // Property
        [SerializeField] private GameObject _impactPrefab;


        // Content
        public override void OnArrived()
        {
            Destroy(transform.GetChild(0).gameObject);
            Speed = 0f;

            var impact = Instantiate(_impactPrefab);
            impact.transform.SetParent(transform);
            impact.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            impact.transform.localScale = Vector3.one;

            if (TryGetImpactTime(out var impactTime))
            {
                StartCoroutine(DestroyAfter());
                impact.SetActive(true);
            }
            else
                Destroy(gameObject);


            bool TryGetImpactTime(out float impactTime)
            {
                if (!impact.TryGetComponent<Animator>(out var animator))
                {
                    Debug.LogWarning(
                        Ctx($"{nameof(_impactPrefab)}이(가) Animator 컴포넌트를 가지고 있지 않기 때문에 Impact 애니메이션을 재생하지 않습니다."));

                    impactTime = 0;
                    return false;
                }

                var impactAnimName = "Projectile_Impact";
                if (!animator.TryFindClip(impactAnimName, out var clip))
                {
                    Debug.LogWarning(
                        Ctx($"[CrowProjectile] {nameof(_impactPrefab)}의 애니메이터가 {impactAnimName} 애니메이션을 가지고 있지 않기 때문에 Impact 애니메이션을 재생하지 않습니다."));

                    impactTime = 0;
                    return false;
                }

                impactTime = clip.length;
                return true;
            }

            IEnumerator DestroyAfter()
            {
                var playtime = 0f;

                while (playtime < impactTime)
                {
                    playtime += Time.deltaTime;
                    yield return null;
                }

                Destroy(gameObject);
            }
        }


        private string Ctx(string message) => $"[Crow Projectile] {message}";
    }
}
