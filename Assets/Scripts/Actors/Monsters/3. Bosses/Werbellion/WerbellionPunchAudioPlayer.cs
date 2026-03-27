using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    [System.Obsolete("이제 펀치 사운드는 시전 즉시 재생됩니다.")]
    [RequireComponent(typeof(TriggerContactHandler))]
    internal class WerbellionPunchAudioPlayer : MonsterAudioPlayer
    {
        protected override void Awake()
        {
            if (!enabled) return;

            StandaloneMode = true;
            base.Awake();

            GetComponent<TriggerContactHandler>().CollisionEntered += collosion =>
            {
                if (collosion.CompareTag("Player"))
                    Play("PunchAttack");
            };
        }
    }
}
