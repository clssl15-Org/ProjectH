using System;
using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters.Bosses
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public partial class Belia : Monster<BeliaStats>, ITwinBoss
    {
        // Front
        public enum AttackMode
        {
            Any,
            Slash,
            CurvedArea,
            Dash
        }

        [Header("Belia")]
        [Space]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [SerializeField, Min(0)] private float _targetPlayerRange = 5f;
        [Space]
        [SerializeField] private Weapon _slashWeapon;
        [SerializeField, Min(0)] private float _slashActiveTiming;
        [SerializeField] private float _slashActiveDuration;
        [Space]
        [SerializeField] private GameObject _curveEffectPrefab;
        [SerializeField] private Vector2 _curveEffectWorldPosition;
        [SerializeField, Min(0)] private float _curveEffectLength = 1f;
        [Space]
        [SerializeField] private Weapon _dashWeapon;
        [SerializeField, Min(0)] private float _dashWeaponActiveDuration;
        [SerializeField, Min(0)] private float _dashStartTime = 0f;
        [SerializeField, Min(0)] private float _dashForce = 100f;
        [SerializeField, Min(0)] private float _dashMass = 0.5f;
        [SerializeField, Min(0)] private float _dashDrag = 3f;

        private float _defaultMass;
        private float _defaultDrag;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        public bool IsExhausted
        {
            get => _isExhausted;
            set
            {
                _isExhausted = value;
                IgnorePlayerInteraction = value;
            }
        } bool _isExhausted = false;

        internal override GameObject DetectedPlayer => _player?.gameObject;


        // Internal
        private class BeliaBrain : MonsterBrain
        {
            public BeliaBrain(Belia owner) : base(owner)
            {
                Blackboard.Properties[ITwinBoss.IsAwake] = false;

                AddChild(new Alive()
                    .AddChild(new Idle(ITwinBoss.IsAwake))
                    .AddChild(new Awaken(ITwinBoss.IsAwake)
                        .AddChild(new ValidPlatform() { HierarchyMode = HierarchyMode.Sequence }
                            .AddChild(new Engaged()
                                {
                                    TargetAttackRange = owner._targetPlayerRange,
                                    LowerRangeTolerance = 0.5f,
                                    UpperRangeTolerance = 0.5f,
                                }
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new BeliaAttackBrain())
                        )
                        .AddChild(new NotValidPlatform())
                    )
                );
                AddChild(new TwinBossExhaustedBrain("Exhausted"));
                AddChild(new Dead() { IsSelectable = false });
            }
        }

        private class BeliaActionController : MonsterActionController
        {
            public BeliaActionController(Belia monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction("SlashAttack")
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(
                        monster._slashWeapon,
                        monster._slashActiveTiming,
                        monster._slashActiveDuration))
                );
                AddChild(new MonsterAction("CurvedAreaAttack")
                    .AddAnimationComponent(
                        "CurvedAreaAttackStart",
                        out var curvedAreaAttackEnter
                    )
                    .AddComponent(new BeliaCurvedAreaAttackAction(
                            monster._curveEffectPrefab,
                            monster._curveEffectWorldPosition,
                            monster._curveEffectLength),
                        out var curvedAreaAttackAction,
                        after: new(curvedAreaAttackEnter)
                    )
                    .AddAnimationComponent(
                        "CurvedAreaAttackEnd",
                        after: new(curvedAreaAttackAction)
                    )
                );
                AddChild(new MonsterAction("DashAttack")
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddDelay(monster._dashStartTime, out var delay)
                    .AddComponent(new AttackWithWeapon(
                        monster._dashWeapon,
                        0f,
                        monster._dashWeaponActiveDuration),
                        after: new(delay)
                    )
                    .AddComponent(new Do(false)
                        .OnOpening(() =>
                        {
                            monster.Rigidbody.mass = monster._dashMass;
                            monster.Rigidbody.drag = monster._dashDrag;
                        })
                        .OnInterrupted(_ =>
                        {
                            monster.Rigidbody.mass = monster._defaultMass;
                            monster.Rigidbody.drag = monster._defaultDrag;
                        })
                        .SetInterruptPriotiy(InterruptPriority.Low),
                        after: new(delay)
                    )
                    .AddComponent(new BeliaDashAttackAction(
                        monster._dashForce),
                        after: new(delay)
                    )
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
            if (!_curveEffectPrefab)
                throw new InvalidOperationException(
                    $"{nameof(Belia)}은(는) '{nameof(_curveEffectPrefab)}'을(를) 가지고 있어야 합니다.");

            base.Awake();

            _defaultMass = Rigidbody.mass;
            _defaultDrag = Rigidbody.drag;
        }

        protected override void Start()
        {
            base.Start();

            _curveEffectPrefab.GetComponent<Weapon>().AttackPower =
                StatsInfo.CurvedAreaAttackPower;

            if (_slashWeapon)
            {
                _slashWeapon.AttackPower = StatsInfo.SlashAttackPower;
                _slashWeapon.gameObject.SetActive(false);
            }
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_slashWeapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            if (_dashWeapon)
            {
                _dashWeapon.AttackPower = StatsInfo.DashAttackPower;
                _dashWeapon.gameObject.SetActive(false);
            }
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_dashWeapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            ActionController = new BeliaActionController(this);
            ActionController.Enter();

            Brain = new BeliaBrain(this);


            // ------- Debug -------
            if (_useTargetPlayer
                && _targetPlayer
                && _targetPlayer.TryGetComponent<IPlayer>(out var player))
                InitializePlayer(player);

            if (_autoAwake)
                Commence();
        }

        public void Commence()
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
            NotifyConditionImmediately(new MonsterConditionData(MonsterCondition.Heal));
        }

        public void SetToDead()
        {
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(nameof(Dead), null, EntryPolicy.Unconditional, RerunPolicy.Restart)
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
        [CustomEditor(typeof(Belia))]
        private class BeliaEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (Belia)base.target;

                if (!target._autoAwake && GUILayout.Button("Awake"))
                    target.Commence();
            }
        }
#endif
    }
}
