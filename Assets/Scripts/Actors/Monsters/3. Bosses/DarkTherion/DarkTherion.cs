using System;
using System.Linq;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;
using Infrastructure;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class DarkTherion : Monster<DarkTherionStats>, ITwinBoss
    {
        // Front
        public enum AttackMode
        {
            Any,
            Projectile,
            Bullet,
            Spike
        }

        [Header("Dark Therion")]
        [SerializeField] private Transform[] _movePoints;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _arriveDistanceTolerance = 0.1f;
        [Space]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [Space]
        [SerializeField] private KinematicProjectile _projectilePrefab;
        [SerializeField] private Transform _projectileLaunchPoint;
        [Space]
        [SerializeField, Min(0)] private float _bulletDestroyTime = 10f;
        [Space]
        [SerializeField] private KinematicProjectile _spikePrefab;
        [SerializeField] private Transform[] _spikeSpawnPoints;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        [field: SerializeField] public bool IsExhausted { get; set; } = false;
        internal override GameObject DetectedPlayer => _player?.gameObject;


        // Internal
        private class DarkTherionBrain : MonsterBrain
        {
            public DarkTherionBrain(DarkTherion owner) : base(owner)
            {
                Blackboard.Properties[ITwinBoss.IsAwake] = false;

                AddChild(new Alive()
                    .AddChild(new Idle(ITwinBoss.IsAwake))
                    .AddChild(new Awaken(ITwinBoss.IsAwake)
                        .AddChild(new DarkTherionMoveBrain())
                        .AddChild(new Await(owner.StatsInfo.DelayBeforeAttack))
                        .AddChild(new DarkTherionAttackBrain())
                    )
                );
                AddChild(new TwinBossExhaustedBrain("Exhausted"));
                AddChild(new Dead() { IsSelectable = false });
            }
        }

        private class DarkTherionActionController : MonsterActionController
        {
            public DarkTherionActionController(DarkTherion monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction("ProjectileAttack")
                    .AddAnimationComponent()
                    .AddComponent(new DarkTherionProjectileAttackAction()
                        .SetInitializer(
                            p => p
                                .GetComponent<SpriteSizeHandler>()
                                .Initialize(monster.Configuration, true),
                            p => p
                                .GetComponent<Weapon>()
                                .AttackPower = monster.StatsInfo.ProjectileAttackPower))
                );
                AddChild(new MonsterAction("BulletAttack")
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new DarkTherionBulletAttackAction()
                        .SetInitializer(
                            p => p
                                .GetComponent<SpriteSizeHandler>()
                                .Initialize(monster.Configuration, true),
                            p => p
                                .GetComponent<Projectile>()
                                .Initialize(monster.PlatformManager),
                            p => p
                                .GetComponent<Weapon>()
                                .AttackPower = monster.StatsInfo.BulletAttackPower))
                );
                AddChild(new MonsterAction("SpikeAttack")
                    .AddAnimationComponent()
                    .AddComponent(new SpikeAttackAction(
                        monster._spikePrefab,
                        monster._spikeSpawnPoints.Select(p => p.transform),
                        SpikeAttackAction.SpawnPointType.Local,
                        monster.StatsInfo.ProjectileSpeed,
                        monster.StatsInfo.ProjectileFireGap)
                        .SetInitializer(
                            p => p
                                .GetComponent<SpriteSizeHandler>()
                                .Initialize(monster.Configuration, true),
                            p => p
                                .GetComponent<Weapon>()
                                .AttackPower = monster.StatsInfo.SpikeAttackPower)
                    )
                    .AddDelay(
                        5f,
                        InterruptPriority.High)
                );
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddComponent(new HitFlash())
                );
                AddChild(new MonsterAction("Exhausted")
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent()
                );
            }
        }

        private IPlayer _player;

            
        // Content
        public void InitializePlayer(IPlayer player)
        {
            _player = player;
        }

        protected override void Awake()
        {
            if (!_projectilePrefab)
                throw new InvalidOperationException(
                    $"{nameof(DarkTherion)}은(는) '{nameof(_projectilePrefab)}'을(를) 가지고 있어야 합니다.");

            if (!_spikePrefab)
                throw new InvalidOperationException(
                    $"{nameof(DarkTherion)}은(는) '{nameof(_spikePrefab)}'을(를) 가지고 있어야 합니다.");

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            ActionController = new DarkTherionActionController(this);
            ActionController.Enter();

            Brain = new DarkTherionBrain(this);


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
            Brain.Blackboard.Properties[ITwinBoss.IsAwake] = true;
            Brain.Blackboard.Committing = true;
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            StandaloneHitBrain.TryTakeDamage(damageInfo);
        }

        public void Revive(float hpRate)
        {
            HP = Mathf.CeilToInt(hpRate * StatsInfo.MaxHP);
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
            message += $"\nAwaken: {Brain.Blackboard.Properties[ITwinBoss.IsAwake]}";

            return message;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(DarkTherion))]
        private class DarkTherionEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (DarkTherion)base.target;

                if (!target._autoAwake && GUILayout.Button("Awake"))
                    target.DoAwake();
            }
        }
#endif
    }
}
