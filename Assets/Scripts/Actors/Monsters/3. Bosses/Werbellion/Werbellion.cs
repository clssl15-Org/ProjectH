using System;
using System.Linq;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure;
using UnityEngine;
using World;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(StandaloneHitAction), typeof(MonsterAudioPlayer))]
    public partial class Werbellion : Monster<WerbellionStats>, IBoss, IPlayerInitializable
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
        [Space]
        [SerializeField] private AttackMode _attackMode;
        [Space]
        [SerializeField] private Weapon _punchAttackWeapon;
        [SerializeField, Min(0)] private float _punchActiveTiming;
        [SerializeField] private float _punchActiveDuration;
        [Space]
        [SerializeField] private WeaponManager _straightAreaAttackPrefab;
        [SerializeField] float _straightAreaAttackTiming;
        [Space]
        [SerializeField] private KinematicProjectile _spikePrefab;
        [SerializeField] private Transform[] _spikeSpawnPoints;
        [Space]
        [SerializeField] private GameObject _portalAttackSpawnerParent;
        [SerializeField] private WerbellionPortalAttackSpawner[] _portalAttackSpawners;
        [Space]
        [SerializeField] private Weapon _stunAttackweapon;
        [SerializeField, Min(0)] private float _stunAttackActiveTiming;
        [SerializeField] private float _stunAttackActiveDuration;
        [SerializeField, Min(0)] private float _stunAttackTimeScale = 1f;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;
        [SerializeField] private KeyCode _forceClearKey = KeyCode.Alpha2;

        public bool IsInvincible
        {
            get => IgnorePlayerInteraction;
            set => IgnorePlayerInteraction = value;
        }

        public MonsterAudioPlayer AudioPlayer { get; private set; }

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
            public WerbellionActionController(Werbellion monster) : base(monster)
            {
                AddChild(new MonsterAction("Spawn")
                    .AddAnimationComponent(out var spawn)
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("Spawn"))
                    )
                    .AddAnimationComponent("Idle", after: new(spawn))
                );
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction("Teleport")
                    .AddAnimationComponent("TeleportIn", out var teleportIn)
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("TeleportIn"))
                    )
                    .AddComponent(new WerbellionTeleportComponent(), after: new(teleportIn))
                    .AddAnimationComponent("TeleportOut", after: new(teleportIn), interruptPriority: InterruptPriority.High)
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("TeleportOut")),
                        after: new(teleportIn)
                    )
                );

                #region Attacks
                AddChild(new MonsterAction("PunchAttack")
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new Empty())
                    .AddComponent(new AttackWithWeapon(
                        monster._punchAttackWeapon,
                        monster._punchActiveTiming,
                        monster._punchActiveDuration)
                    )
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("PunchAttack"))
                    )
                );

                AddChild(new MonsterAction("StraightAreaAttack")
                    .AddAnimationComponent()
                    .AddComponent(new Empty())
                    .AddComponent(new AttackWithWeapon(
                        monster._straightAreaAttackPrefab,
                        monster._straightAreaAttackTiming)
                    )
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("StraightAreaAttack"))
                    )
                    .AddDelay(1f, interruptPriority: InterruptPriority.High)
                );

                AddChild(new MonsterAction("SpikeAttack")
                    .AddAnimationComponent(
                        "SpikeAttackIn",
                        out var spikeAttackIn
                    )
                    .AddComponent(new SpikeAttackAction(
                        monster._spikePrefab,
                        monster._spikeSpawnPoints.Select(point => point.transform),
                        SpikeAttackAction.SpawnPointType.World,
                        monster.StatsInfo.SpikeSpeed,
                        monster.StatsInfo.SpikeFireGap,
                        standalone: true)
                        .SetInitializer(
                            p => p
                                .GetComponent<SpriteSizeHandler>()
                                .Initialize(monster.Configuration, true),
                            p => p
                                .GetComponent<Weapon>()
                                .AttackPower = monster.StatsInfo.SpikeAttackPower),
                        out var spikeAttack,
                        after: new(spikeAttackIn)
                    )
                    .AddAnimationComponent(
                        "SpikeAttack",
                        after: new(spikeAttackIn)
                    )
                    .AddAnimationComponent(
                        "SpikeAttackOut",
                        after: new(spikeAttack),
                        interruptPriority: InterruptPriority.High
                    )
                );

                AddChild(new MonsterAction("PortalAttack")
                    // 공중으로 텔레포트
                    .AddAnimationComponent("TeleportIn", out var portal_teleportIn_a)
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("TeleportIn"))
                    )
                    .AddComponent(new WerbellionTeleportComponent(), after: new(portal_teleportIn_a))
                    .AddAnimationComponent("TeleportOut", out var portal_teleportOut_a, after: new(portal_teleportIn_a))
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("TeleportOut")),
                        after: new(portal_teleportIn_a)
                    )

                    // 공격
                    .AddAnimationComponent(after: new(portal_teleportOut_a))
                    .AddComponent(new WerbellionPortalAttackAction(
                            monster.PlatformManager,
                            monster._portalAttackSpawnerParent,
                            monster._portalAttackSpawners,
                            () => monster._player.transform.position
                        ),
                        out var portal_attacked,
                        after: new(portal_teleportOut_a)
                    )

                    // 지상으로 텔레포트
                    .AddAnimationComponent("TeleportIn", out var portal_teleportIn_b, after: new(portal_attacked))
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("TeleportIn")),
                        after: new(portal_attacked)
                    )
                    .AddComponent(new WerbellionTeleportComponent(), after: new(portal_teleportIn_b))
                    .AddAnimationComponent("TeleportOut", after: new(portal_teleportIn_b), interruptPriority: InterruptPriority.High)
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("TeleportOut")),
                        after: new(portal_teleportIn_b)
                    )
                );

                AddChild(new MonsterAction("StunAttack")
                    .AddComponent(new SetTimeScale(monster._stunAttackTimeScale))
                    .AddComponent(new Empty())
                    .AddComponent(new AttackWithWeapon(
                        monster._stunAttackweapon,
                        monster._stunAttackActiveTiming,
                        monster._stunAttackActiveDuration)
                    )
                    .AddAnimationComponent(
                        "StunAttack",
                        interruptPriority: InterruptPriority.High
                    )
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("StunAttack"))
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
                    .AddAnimationComponent("Dead", after: new(dead_teleportOut), interruptPriority: InterruptPriority.High)
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("Exhausted"))
                    )
                );
                AddChild(new MonsterAction("DeadGround")
                    .AddComponent(new Do(true, () => Owner.Rigidbody.gravityScale = 1f))
                    .AddAnimationComponent("Dead")
                    .AddComponent(new Do(true)
                        .OnOpening(() => monster.AudioPlayer.Play("Exhausted"))
                    )
                );
            }
        }

        private IPlayer _player;


        // Content
        protected override void Awake()
        {
            if (!_spikePrefab)
                throw new InvalidOperationException(
                    $"{nameof(Werbellion)}은(는) '{nameof(_spikePrefab)}'을(를) 가지고 있어야 합니다.");

            base.Awake();
            AudioPlayer = GetComponent<MonsterAudioPlayer>();
        }

        public void InitializePlayer(IPlayer player)
        {
            _player = player;
        }

        protected override void Start()
        {
            base.Start();

            if (_punchAttackWeapon)
            {
                _punchAttackWeapon.AttackPower = StatsInfo.PunckAttackPower;
                _punchAttackWeapon.gameObject.SetActive(false);
            }
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_punchAttackWeapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            if (_stunAttackweapon)
            {
                _stunAttackweapon.AttackPower = 0;
                _stunAttackweapon.gameObject.SetActive(false);
            }
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_stunAttackweapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            foreach (var spawner in _portalAttackSpawners)
                spawner
                    .Initialize(PlatformManager, () => _player.transform.position)
                    .SetInitializer(p => p.GetComponent<Weapon>().AttackPower = StatsInfo.PortalAttackPower);

            ActionController = new WerbellionActionController(this);
            ActionController.Enter();

            Brain = new WerbellionBrain(this);
            StandaloneHitBrain.DoKnockback = false;

            // 시작 애니메이션 지연 방지
            SpriteRenderer.enabled = false;
            new Timer(0.1f, _ =>
            {
                if (this && SpriteRenderer)
                    SpriteRenderer.enabled = true;
            });


            // ------- Debug -------
            if (_useTargetPlayer
                && _targetPlayer
                && _targetPlayer.TryGetComponent<IPlayer>(out var player))
                InitializePlayer(player);

            if (_autoAwake.Resolve(false))
                Commence();
        }

        public void Commence()
        {
            Brain.Blackboard.Properties[IsAwake] = true;
            Brain.Blackboard.IsCommitting = true;
        }

        protected override void Update()
        {
            if (Input.GetKeyDown(_forceClearKey.Resolve()))
            {
                OnDamaged(new DamageInfo(int.MaxValue) { HasKnockback = false });
                return;
            }

            base.Update();
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            if (!(bool)Brain.Blackboard.Properties[IsAwake]) return;
            StandaloneHitBrain.TryTakeDamage(damageInfo);
        }

        protected override string GetDisplayContent()
        {
            var message = base.GetDisplayContent();

            message += "----------------";
            message += $"\nAwaken: {(bool)Brain.Blackboard.Properties[IsAwake]}";

            return message;
        }

        //internal override void Die() => Die(false);

#if UNITY_EDITOR
        [CustomEditor(typeof(Werbellion))]
        private class WerbellionEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (Werbellion)base.target;

                if (!target._autoAwake && GUILayout.Button("Awake"))
                    target.Commence();
            }
        }
#endif
    }
}
