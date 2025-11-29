using System.Linq;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using Infrastructure.StateMachines.BT;
using UnityEngine;
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
        [SerializeField] private Configuration _configuration;
        [SerializeField] private KinematicProjectile _projectilePrefab;
        [SerializeField] private Projectile _spikePrefab;
        [Space]
        [SerializeField, Min(0)] private float _bulletDestroyTime = 10f;
        [SerializeField] private Transform[] _spikeSpawnPoints;
        [Space]
        [SerializeField] private Transform[] _movePoints;
        [SerializeField] private float _acceleration = 20f;
        [SerializeField] private float _deceleration = 30f;
        [SerializeField] private float _arriveDistanceTolerance = 0.1f;
        [Space]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        public bool IsExhausted { get; set; } = false;
        internal override GameObject DetectedPlayer => _player?.gameObject;


        // Internal
        private class DarkTherionBrain : MonsterBrain
        {
            public DarkTherionBrain(DarkTherion darkTherion) : base(darkTherion)
            {
                Blackboard.Properties[ITwinBoss.IsAwake] = false;

                AddChild(new Alive()
                    .AddChild(new Idle(ITwinBoss.IsAwake))
                    .AddChild(new Awaken(ITwinBoss.IsAwake)
                        .AddChild(new DarkTherionMoveBrain())
                        .AddChild(new Await(darkTherion.StatsInfo.DelayBeforeAttack))
                        .AddChild(new DarkTherionAttackBrain())
                    )
                );
                AddChild(new TwinBossExhaustedBrain());
                AddChild(new Dead() { IsSelectable = false });
            }
        }

        private class DarkTherionActionController : MonsterActionController
        {
            public DarkTherionActionController(DarkTherion darkTherion) : base(darkTherion)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction("ProjectileAttack")
                    .AddAnimationComponent()
                    .AddComponent(new DarkTherionProjectileAttackAction()));
                AddChild(new MonsterAction("BulletAttack")
                    .AddAnimationComponent(interruptAllOnDeactivate: true)
                    .AddComponent(new DarkTherionBulletAttackAction()));
                AddChild(new MonsterAction("SpikeAttack")
                    .AddAnimationComponent()
                    .AddComponent(new SpikeAttackAction(
                        darkTherion._spikePrefab,
                        darkTherion._spikeSpawnPoints.Select(p => (Vector2)p.transform.localPosition),
                        darkTherion.StatsInfo.ProjectileSpeed,
                        darkTherion.StatsInfo.ProjectileFireGap))
                    .AddDelayComponent(5f, true));
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelayComponent()
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction("Exhausted")
                    .AddAnimationComponent());
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
