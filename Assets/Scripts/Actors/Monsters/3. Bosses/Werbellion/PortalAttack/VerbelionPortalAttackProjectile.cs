using Actors.Monsters.Actions;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public class VerbelionPortalAttackProjectile : KinematicProjectile
    {
        [SerializeField] string _explodeAnimName = "PortalExploding";

        public override void OnArrived()
        {
            Rigidbody.velocity = Vector2.zero;

            var animator = GetComponentInChildren<Animator>();
            if (!animator)
            {
                base.OnArrived();
                return;
            }

            var player = new MonsterAnimationPlayer(animator);
            player.Play(new(
                AnimationName: _explodeAnimName,
                Callback: _ =>
                {
                    player.Dispose();
                    if (gameObject) base.OnArrived();
                }));

        }
    }
}
