using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters.Bosses
{
    public partial class Cerberus : Monster<CerberusStats>
    {
        internal class CerberusAttackBrain : BTNode<IMonsterInternal, MonsterBlackboard>
        {
            private Cerberus Cerberus => (Cerberus)Owner;


            public CerberusAttackBrain() : base(name: MonsterActionType.Attack.ToString()) { }

            protected override void OnOpen(object[] _)
            {
                var isNear = Mathf.Abs(
                    Cerberus._player.transform.position.x
                    - Cerberus.transform.position.x)
                    <= Cerberus.StatsInfo.NearDistance;

                AttackMode mode;

                if (Cerberus._attackMode == AttackMode.Any)
                {
                    if (isNear)
                        mode = Random.Range(0, 100) switch
                        {
                            < (70)      => AttackMode.Bite,
                            < (70 + 15) => AttackMode.Drop,
                            _           => AttackMode.Ambush,
                        };
                    else
                        mode = Random.Range(0, 100) switch
                        {
                            < 50 => AttackMode.Drop,
                            _    => AttackMode.Ambush,
                        };
                }
                else
                    mode = Cerberus._attackMode;

                var attackName = mode.ToString() + "Attack";


                if (!Owner.TryDoAction(new(
                    Name: attackName,
                    Callback: result => Complete(result)),
                    out var reason,
                    allowRestart: true))
                {
                    Debug.LogWarning(Owner.FormatLogMessage(
                        $"{attackName} 행동에 실패하였기 때문에 {nameof(CerberusAttackBrain)} 상태로 진입할 수 없습니다.\n{reason}"), Cerberus);

                    Complete(false);
                }
            }
        }
    }
}
