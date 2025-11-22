using System;
using System.Linq;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters.Stage3Bosses
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

        [Header("Werbellion")]
        [SerializeField] private Transform[] _movePoints;
        [SerializeField] private AttackMode _attackMode;
        [Space]
        [SerializeField] private GameObject _straightAreaAttackPrefab;
        [SerializeField] float _straightAreaAttackTiming;
        [Space]
        [SerializeField] private Projectile _spikePrefab;
        [SerializeField] private Transform[] _spikeSpawnPoints;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        internal override GameObject DetectedPlayer => _player?.gameObject;
        internal override PlatformDetector PlatformDetector => throw new InvalidOperationException(
            $"{nameof(Werbellion)}은(는) {nameof(PlatformDetector)} 프로퍼티를 사용하지 않습니다.");


        // Internal
        private class WerbellionBrain : MonsterBrain
        {
            public WerbellionBrain(IMonsterInternal owner) : base(owner)
            {
                Blackboard.Properties[ITwinBoss.IsAwaken] = false;

                AddChild(new Alive(opened: () => owner.Rigidbody.gravityScale = 0f)
                    .AddChild(new Idle("Spawn", haltOnActionEnd: false))
                    .AddChild(new Awaken()
                        .AddChild(new WerbellionTeleportBrain())
                        .AddChild(new Await(
                            MonsterActionType.Idle,
                            owner.StatsInfo.AttackCooltime)
                        )
                        .AddChild(new WerbellionAttackBrain(0))
                        .AddChild(new WerbellionAttackBrain(1))
                        .AddChild(new WerbellionAttackBrain(2))
                    )
                );
                AddChild(new Dead(
                    opening: () => owner.Rigidbody.gravityScale = 1f)
                    { DestroyOwnerOnCompleted = false }
                );
            }
        }

        private class WerbellionActionController : MonsterActionController
        {
            public WerbellionActionController(Werbellion werbellion) : base(werbellion)
            {
                AddChild(new MonsterAction("Spawn")
                    .AddAnimationComponent(out var spawn)
                    .AddAnimationComponent("Idle", after: new(spawn)));
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction("Teleport")
                    .AddAnimationComponent("TeleportIn", out var teleportIn)
                    .AddComponent(new WerbellionTeleportComponent(), after: new(teleportIn))
                    .AddAnimationComponent("TeleportOut", after: new(teleportIn)));
                AddChild(new MonsterAction("PunchAttack")
                    .AddAnimationComponent());
                AddChild(new MonsterAction("StraightAreaAttack")
                    .AddAnimationComponent()
                    .AddComponent(new AttackWithWeapon(
                        werbellion._straightAreaAttackPrefab,
                        werbellion._straightAreaAttackTiming))
                    .AddDelayComponent(1f, interruptAllOnDeactivate: true));
                AddChild(new MonsterAction("SpikeAttack")
                    .AddAnimationComponent(
                        "SpikeAttackIn",
                        out var spikeAttackIn
                    )
                    .AddComponent(new SpikeAttackAction(
                            werbellion._spikePrefab,
                            werbellion._spikeSpawnPoints.Select(point => (Vector2)point.transform.localPosition),
                            werbellion.StatsInfo.SpikeSpeed,
                            werbellion.StatsInfo.SpikeFireGap),
                        after: new(spikeAttackIn)
                    )
                    .AddAnimationComponent(
                        MonsterActionType.Idle.ToString(),
                        after: new(spikeAttackIn)
                    )
                    .AddDelayComponent(
                        5f,
                        out var spikeAttackAction,
                        after: new(spikeAttackIn)
                    )
                    .AddAnimationComponent(
                        "SpikeAttackOut",
                        interruptAllOnDeactivate: true,
                        after: new(spikeAttackAction))
                    );
                AddChild(new MonsterAction("PortalAttack")
                    .AddAnimationComponent());
                AddChild(new MonsterAction("StunAttack")
                    .AddAnimationComponent(
                        "StunAttackIn",
                        out var stunAttackIn
                    )
                    .AddDelayComponent(
                        werbellion.StatsInfo.StunAttackTime,
                        out var stunAttack,
                        after: new(stunAttackIn)
                    )
                    .AddAnimationComponent(
                        "StunAttackOut",
                        after: new(stunAttack)
                    ));
               AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelayComponent()
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
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

        protected override void Update()
        {
            base.Update();  
        }

        public void DoAwake()
        {
            Brain.Blackboard.Properties[ITwinBoss.IsAwaken] = true;
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
            message += $"\nAwaken: {(bool)Brain.Blackboard.Properties[ITwinBoss.IsAwaken]}";

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
