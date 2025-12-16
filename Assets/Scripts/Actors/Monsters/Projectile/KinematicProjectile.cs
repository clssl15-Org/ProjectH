using UnityEngine;

namespace Actors.Monsters
{
    public class KinematicProjectile : Projectile
    {
        protected Vector3 Direction { get; set; }
        protected float Speed { get; set; }

        private bool _stopWhenArrived;


        private void Start()
        {
            Rigidbody.isKinematic = true;
            Rigidbody.useFullKinematicContacts = true;
        }

        public void Launch(Vector2 direction, float speed, bool stopWhenArrived = true)
        {
            Direction = direction.normalized;
            Speed = speed;
            _stopWhenArrived = stopWhenArrived;
        }

        protected override void Update()
        {
            if (!_stopWhenArrived || !HasArrived)
                transform.position += Time.deltaTime * Speed * Direction;

            base.Update();
        }
    }
}
