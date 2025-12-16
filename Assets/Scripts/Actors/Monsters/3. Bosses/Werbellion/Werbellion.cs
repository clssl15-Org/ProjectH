using System;
using System.Linq;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using World;
using Infrastructure.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class Werbellion : Monster<WerbellionStats>
    {
        // Front
        public enum AttackMode
        {
            Any = 0,
            Punch,
            StraightArea,
            Spike,
            Portal,
            Stun,
        }

        private const string IsAwake = nameof(IsAwake);
        private const string IsOnAir = nameof(IsOnAir);


        [Header("Werbellion")]
        [SerializeField] private Transform[] _groundPoints;
        [SerializeField] private Transform[] _airPoints;
        [SerializeField] private AttackMode _attackMode;
        [Space]
        [SerializeField] private GameObject _straightAreaAttackPrefab;
        [SerializeField] float _straightAreaAttackTiming;
        [Space]
        [SerializeField] private KinematicProjectile _spikePrefab;
        [SerializeField] private Transform[] _spikeSpawnPoints;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        internal override GameObject DetectedPlayer => _player?.gameObject;
        internal override PlatformDetector PlatformDetector => throw new InvalidOperationException(
            $"{nameof(Werbellion)}은(는) '{nameof(PlatformDetector)}' 프로퍼티를 사용하지 않습니다.");


        // Internal
        private class WerbellionBrain : MonsterBrain
        {
            public WerbellionBrain(IMonsterInternal owner) : base(owner)
            {
                Blackboard.Properties[IsAwake] = false;
                Blackboard.Properties[IsOnAir] = false;

                AddChild(new Alive(opened: () => owner.Rigidbody.gravityScale = 0f)
                    .AddChild(new Idle("Spawn", IsAwake, haltOnActionEnd: false))
                    .AddChild(new Awaken(IsAwake)
                        .AddChild(new WerbellionTeleportBrain())
                        .AddChild(new WerbellionAttackBrain())
                        .AddChild(new Await(
                            MonsterActionType.Idle,
                            owner.StatsInfo.AttackCooltime)
                        )
                    )
                );
                AddChild(new WerbellionDeadBrain());
            }
        }

        private class WerbellionActionController : MonsterActionController
        {
            public WerbellionActionController(Werbellion werbellion) : base(werbellion)
            {
                AddChild(new MonsterAction("Spawn")
                    .AddAnimationComponent(out var spawn)
                    .AddAnimationComponent("Idle", after: new(spawn))
                );
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction("Teleport")
                    .AddAnimationComponent("TeleportIn", out var teleportIn)
                    .AddComponent(new WerbellionTeleportComponent(), after: new(teleportIn))
                    .AddAnimationComponent("TeleportOut", after: new(teleportIn))
                );

                #region Attacks
                AddChild(new MonsterAction("PunchAttack")
                    .AddAnimationComponent()
                );

                AddChild(new MonsterAction("StraightAreaAttack")
                    .AddAnimationComponent()
                    .AddComponent(new AttackWithWeapon(
                        werbellion._straightAreaAttackPrefab,
                        werbellion._straightAreaAttackTiming))
                    .AddDelay(1f, interruptAllOnDeactivate: true)
                );

                AddChild(new MonsterAction("SpikeAttack")
                    .AddAnimationComponent("SpikeAttackIn", out var spikeAttackIn)
                    .AddComponent(new SpikeAttackAction(
                        werbellion._spikePrefab,
                        werbellion._spikeSpawnPoints.Select(point => (Vector2)point.transform.position),
                        SpikeAttackAction.SpawnPointType.World,
                        werbellion.StatsInfo.SpikeSpeed,
                        werbellion.StatsInfo.SpikeFireGap),
                        out var spikeAttack,
                        after: new(spikeAttackIn)
                    )
                    .AddAnimationComponent(
                        MonsterActionType.Idle.ToString(),
                        after: new(spikeAttackIn)
                    )
                    .AddAnimationComponent(
                        "SpikeAttackOut",
                        after: new(spikeAttack),
                        interruptAllOnDeactivate: true
                    )
                );

                AddChild(new MonsterAction("PortalAttack")
                    // 공중으로 텔레포트
                    .AddAnimationComponent("TeleportIn", out var portal_teleportIn_a)
                    .AddComponent(new WerbellionTeleportComponent(), after: new(portal_teleportIn_a))
                    .AddAnimationComponent("TeleportOut", out var portal_teleportOut_a, after: new(portal_teleportIn_a))

                    // 공격
                    .AddAnimationComponent(after: new(portal_teleportOut_a))
                    .AddComponent(new Empty(), after: new(portal_teleportOut_a)) // 공격 수행
                    .AddComponent(new Delay(1), out var portal_attacked, after: new(portal_teleportOut_a)) // 타이머 (임시)

                    // 지상으로 텔레포트
                    .AddAnimationComponent("TeleportIn", out var portal_teleportIn_b, after: new(portal_attacked))
                    .AddComponent(new WerbellionTeleportComponent(), after: new(portal_teleportIn_b))
                    .AddAnimationComponent("TeleportOut", after: new(portal_teleportIn_b), interruptAllOnDeactivate: true)
                );

                AddChild(new MonsterAction("StunAttack")
                    .AddAnimationComponent(
                        "StunAttackIn",
                        out var stunAttackIn
                    )
                    .AddDelay(
                        werbellion.StatsInfo.StunAttackTime,
                        out var stunAttack,
                        after: new(stunAttackIn)
                    )
                    .AddAnimationComponent(
                        "StunAttackOut",
                        after: new(stunAttack)
                    )
                );
                #endregion

                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddComponent(new HitFlash())
                );
                AddChild(new MonsterAction("DeadAir")
                    .AddAnimationComponent("TeleportIn", out var dead_teleportIn)
                    .AddComponent(new WerbellionTeleportComponent(), after: new(dead_teleportIn))
                    .AddAnimationComponent("TeleportOut", out var dead_teleportOut, after: new(dead_teleportIn))
                    .AddComponent(new Do(true, () => Owner.Rigidbody.gravityScale = 1f), after: new(dead_teleportOut))
                    .AddAnimationComponent("Dead", after: new(dead_teleportOut), interruptAllOnDeactivate: true)
                );
                AddChild(new MonsterAction("DeadGround")
                    .AddComponent(new Do(true, () => Owner.Rigidbody.gravityScale = 1f))
                    .AddAnimationComponent("Dead")
                );
            }
        }

        private IPlayer _player;


        // Content
        public void InitializePlayer(IPlayer player)
        {
            _player = player;
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new WerbellionActionController(this);
            ActionController.Enter();

            Brain = new WerbellionBrain(this);
            StandaloneHitBrain.DoKnockback = false;


            // ------- Debug -------
            if (_useTargetPlayer
                && _targetPlayer
                && _targetPlayer.TryGetComponent<IPlayer>(out var player))
                InitializePlayer(player);

            if (_autoAwake)
                DoAwake();
        }

        public void DoAwake()
        {
            Brain.Blackboard.Properties[IsAwake] = true;
            Brain.Blackboard.Committing = true;
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            StandaloneHitBrain.TryTakeDamage(damageInfo);
        }

        public void Die()
        {
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(nameof(Dead), null, EntryPolicy.Unconditional, RerunPolicy.EnsureRunningAndInjectInputs)
            });
        }

        protected override string GetDisplayContent()
        {
            var message = base.GetDisplayContent();

            message += "----------------";
            message += $"\nAwaken: {(bool)Brain.Blackboard.Properties[IsAwake]}";

            return message;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(Werbellion))]
        private class WerbellionEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (Werbellion)base.target;

                if (!target._autoAwake && GUILayout.Button("Awake"))
                    target.DoAwake();
            }
        }
#endif
    }
}
