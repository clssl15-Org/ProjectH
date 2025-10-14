using UnityEngine;
using UniEngine.StateMachines.BT;

namespace MonsterBT
{
    internal class Engaged : BTNode<IMonster, MonsterBlackboard>
    {
        // Front
        public float TargetAttackRange { get; set; } = 2f;
        public float UpperRangeTolerance { get; set; } = 0.5f;
        public float LowerRangeTolerance { get; set; } = 0.5f;


        // Content
        public enum RangeType
        {
            Contact,
            Ranged
        }
        /// <summary>
        /// 몬스터가 목표 거리 대역을 유지하도록 조정합니다.
        /// </summary>
        /// <param name="monsterAction">진입 시 시도할 몬스터 액션</param>
        /// <param name="rangeType"><see cref="RangeType.Contact"/> 또는 <see cref="RangeType.Ranged"/></param>
        /// <param name="range">
        /// <list type="bullet">
        ///   <item>
        ///     <term>Contact</term>
        ///     <description>
        ///       이 값은 <see cref="UpperRangeTolerance"/>(접촉 허용 오차)로 사용됩니다.
        ///       <see cref="TargetAttackRange"/>는 0으로 고정되며,
        ///       <see cref="LowerRangeTolerance"/>는 기본값을 유지합니다.
        ///     </description>
        ///   </item>
        ///   <item>
        ///     <term>Ranged</term>
        ///     <description>
        ///       이 값은 <see cref="TargetAttackRange"/>(목표 거리)로 사용됩니다.
        ///       허용 오차는 기본값(<see cref="UpperRangeTolerance"/>, <see cref="LowerRangeTolerance"/>)을 따릅니다.
        ///     </description>
        ///   </item>
        /// </list>
        /// <c>null</c>이면 해당 값들은 기본값을 유지합니다.
        /// </param>
        public Engaged(RangeType rangeType = RangeType.Ranged, float? range = null)
        {
            if (range.HasValue && range.Value < 0)
                throw new System.ArgumentOutOfRangeException(
                    $"{nameof(range)}는 0 이상의 값을 가져야 하지만 '{range.Value}'이(가) 입력되었습니다.");

            if (rangeType == RangeType.Contact)
            {
                TargetAttackRange = 0f;

                if (range.HasValue)
                    UpperRangeTolerance = range.Value;
            }
            else
            {
                if (range.HasValue)
                    TargetAttackRange = range.Value;
            }
        }

        protected override void OnTick()
        {
            if (!Owner.DetectedPlayer)
                throw new System.InvalidOperationException($"{nameof(Owner.DetectedPlayer)}이(가) 유효하지 않습니다.");

            var posDelta = (Owner.Direction == Direction.Right
                ? Owner.Collider.bounds.max.x
                : Owner.Collider.bounds.min.x)
                - Owner.DetectedPlayer.transform.position.x;

            Owner.Direction = posDelta > 0 ? Direction.Left : Direction.Right;

            var rangeDelta = Mathf.Abs(posDelta) - TargetAttackRange;

            if (rangeDelta < -LowerRangeTolerance)
            {
                Blackboard.Moved = Owner.TryMove(posDelta > 0 ? Direction.Right : Direction.Left);
                return;
            }
            if (rangeDelta > UpperRangeTolerance)
            {
                Blackboard.Moved = Owner.TryMove();
                return;
            }
            
            Complete();
        }
    }
}
