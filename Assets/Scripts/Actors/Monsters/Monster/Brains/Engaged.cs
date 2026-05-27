using System;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Brains
{
    internal class Engaged : BTNode<IMonsterInternal, MonsterBlackboard>
    {
        // Front
        public float TargetAttackRange { get; set; } = DefaultTargetAttackRange;
        public float UpperRangeTolerance { get; set; } = 0.2f;
        public float LowerRangeTolerance { get; set; } = 0.2f;

        public const float DefaultTargetAttackRange = 3f;
        private const float ActualMoveEpsilon = 0.001f;

        private float _lastPositionX;
        private Direction _lastMoveDirection;
        private float _lastSampleFixedTime;
        private bool _hasMoveSample;
        private bool _isBlocked;

        // Content
        public enum RangeType
        {
            Contact,
            Ranged
        }
        /// <summary>
        /// 몬스터가 목표 거리 대역을 유지하도록 조정합니다.
        /// </summary>
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
                throw new ArgumentOutOfRangeException(
                    $"{nameof(range)}은(는) 0 이상의 값을 가져야 하지만 '{range.Value}'이(가) 입력되었습니다.");

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

        protected override void OnOpen(object[] _)
        {
            _lastPositionX = Owner.transform.position.x;
            _lastMoveDirection = Direction.Center;
            _lastSampleFixedTime = Time.fixedTime;
            _hasMoveSample = false;
            _isBlocked = false;
            Blackboard.IsMoved = false;
        }

        protected override void OnTick()
        {
            if (!Owner.DetectedPlayer)
                throw new InvalidOperationException(Owner.FormatLogMessage(
                    $"{nameof(Owner.DetectedPlayer)}이(가) 유효하지 않습니다."));

            var selfIsLeftOfPlayer = Owner.transform.position.x <= Owner.DetectedPlayer.transform.position.x;
            Owner.Direction = selfIsLeftOfPlayer ? Direction.Right : Direction.Left;

            var posDelta = Owner.DetectedPlayer.transform.position.x - Owner.transform.position.x;
            if (!selfIsLeftOfPlayer) posDelta *= -1;

            var rangeDelta = posDelta - TargetAttackRange;
            if (rangeDelta < -LowerRangeTolerance)
            {
                TryMoveAndUpdateActualMovement(Owner.Direction.Flip());
                return;
            }
            if (rangeDelta > UpperRangeTolerance)
            {
                TryMoveAndUpdateActualMovement(Owner.Direction);
                return;
            }
            
            Complete();
        }

        private void TryMoveAndUpdateActualMovement(Direction direction)
        {
            var currentPositionX = Owner.transform.position.x;
            var canMove = Owner.TryMove(direction);

            if (!canMove)
            {
                UpdateMoveSample(currentPositionX, direction);
                Blackboard.IsMoved = false;
                _isBlocked = true;
                Owner.StopMoving();
                return;
            }

            if (!_hasMoveSample)
            {
                UpdateMoveSample(currentPositionX, direction);
                Blackboard.IsMoved = true;
                _isBlocked = false;
                return;
            }

            if (Mathf.Approximately(Time.fixedTime, _lastSampleFixedTime))
            {
                Blackboard.IsMoved = !_isBlocked;

                if (_isBlocked)
                    Owner.StopMoving();

                return;
            }

            var deltaX = currentPositionX - _lastPositionX;
            var moved = _lastMoveDirection switch
            {
                Direction.Left => deltaX < -ActualMoveEpsilon,
                Direction.Right => deltaX > ActualMoveEpsilon,
                _ => Mathf.Abs(deltaX) > ActualMoveEpsilon
            };

            UpdateMoveSample(currentPositionX, direction);
            Blackboard.IsMoved = moved;
            _isBlocked = !moved;

            if (!moved)
                Owner.StopMoving();
        }

        private void UpdateMoveSample(float positionX, Direction direction)
        {
            _lastPositionX = positionX;
            _lastMoveDirection = direction;
            _lastSampleFixedTime = Time.fixedTime;
            _hasMoveSample = true;
        }
    }
}
