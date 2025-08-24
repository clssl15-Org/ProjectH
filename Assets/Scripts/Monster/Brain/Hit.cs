using System;
using System.Linq;
using UniEngine.StateMachines.BT;
using UnityEngine;

namespace MonsterBT
{
    public class Hit : BTNode<Monster, MonsterBlackboard>
    {
        // Front
        public float InvincibleTime { get; set; }

        // Internal
        private float remainingTime;


        // Content
        public Hit(float invincibleTime = 1)
        {
            IsSelectable = false;
            AbortPolicy = AbortPolicies.LowerPriority;
            HierarchyMode = HierarchyMode.Selector; // Parallel
            InvincibleTime = invincibleTime;
        }

        protected override bool CheckCondition() => !Owner.IsTakingDamage;

        protected override void OnOpen(object[] inputs)
        {
            if (inputs == null)
                throw new ArgumentNullException(nameof(inputs), CtxHit("Inputs 인자는 null일 수 없습니다."));
            if (inputs.Length != 1 ||  inputs[0] is not int damage)
                throw new ArgumentException(CtxHit($"Inputs 인자는 damage(int)를 담고 있는 크기 1의 배열이여야 합니다.\n" +
                    $"입력값: {string.Join(", ", inputs.Select(i => i?.ToString() ?? null))}"), nameof(inputs));

            Owner.HP -= damage;
            remainingTime = InvincibleTime;

            Owner.DoAction(MonsterAction.Hit);
        }

        protected override void OnTick()
        {
            remainingTime -= Time.deltaTime;

            if (remainingTime <= 0)
                Complete();
        }

        private string CtxHit(string message) => $"BTNode.Hit: {message}";
    }
}
