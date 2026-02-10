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
        private Transform rootTransform;

        private void Awake()
        {
            _contactHandler = GetComponent<TriggerContactHandler>();
            _contactHandler.TargetTags = new[] { "Player" };

            rootTransform = this.transform.root.GetComponent<Transform>();

            _contactHandler.CollisionEntered += c =>
            {
                if (!c.TryGetComponent<IDamageable>(out var receiver))
                    return;

                receiver.TakeDamage(
                    AttackPower,
                    DoKnockback
                        ? (c.transform.position - (transform.position + rootTransform.position)/2).ToDirection()
                        : Direction.Center,
                    KnockbackForce);
            };
        }
    }
}
