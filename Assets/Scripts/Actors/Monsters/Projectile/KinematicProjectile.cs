using UnityEngine;

namespace Actors.Monsters
{
    public class KinematicProjectile : Projectile
    {
        protected Vector3 Direction { get; set; }
        protected float Speed { get; set; }


        private void Start()
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useFullKinematicContacts = true;
        }

        public void Launch(Vector2 direction, float speed)
        {
            Direction = direction.normalized;
            Speed = speed;
        }

        protected override void Update()
        {
            transform.position += Time.deltaTime * Speed * Direction;
            base.Update();
        }
    }
}
