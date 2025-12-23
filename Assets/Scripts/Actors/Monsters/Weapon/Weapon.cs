using Infrastructure;
using Rules;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(TriggerContactHandler))]
    public class Weapon : MonoBehaviour, IWeapon
    {
        [field: SerializeField] public int AttackPower { get; set; } = 1;
        [field: SerializeField] public bool DoKnockback { get; set; } = true;
        public float? KnockbackForce { get; set; } = null;

        private TriggerContactHandler _contactHandler;

        private void Awake()
        {
            _contactHandler = GetComponent<TriggerContactHandler>();
            _contactHandler.TargetTags = new[] { "Player" };

            _contactHandler.CollisionEntered += c =>
            {
                if (!c.TryGetComponent<IDamageable>(out var receiver))
                    return;

                receiver.TakeDamage(
                    AttackPower,
                    DoKnockback
                        ? Direction.Center
                        : (c.transform.position - transform.position).ToDirection(),
                    KnockbackForce);
            };
        }
    }
}
