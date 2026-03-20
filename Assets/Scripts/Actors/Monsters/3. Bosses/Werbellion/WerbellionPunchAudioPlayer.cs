using Infrastructure;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(TriggerContactHandler))]
    internal class WerbellionPunchAudioPlayer : MonsterAudioPlayer
    {
        protected override void Awake()
        {
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
