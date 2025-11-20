using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class Await : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        public float Duration { get; set; }
        private float _remaining;

        public Await(float duration) => Duration = duration;

        protected override void OnOpen(params object[] _)
        {
            _remaining = Duration;
        }

        protected override void OnTick()
        {
            _remaining -= Time.deltaTime;
            if (_remaining <= 0f) Complete();
        }
    }
}
