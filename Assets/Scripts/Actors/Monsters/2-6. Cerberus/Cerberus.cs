using System;
using System.Linq;
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
    public partial class Cerberus : Monster<CerberusStats>
    {
        // Front
        public enum AttackMode
        {
            Any,
            Bite,
            Drop,
            Ambush,
        }

        [Header("Cerberus")]
        [SerializeField] private AttackMode _attackMode = AttackMode.Any;
        [Space]
        [SerializeField] private Weapon _biteWeapon;
        [SerializeField, Min(0)] private float _biteWeaponActiveTiming;
        [SerializeField] private float _biteWeaponActiveDuration;
        [Space]
        [SerializeField] private FallingStoneManager _fallingStoneManager;
        [SerializeField] private AmbushAttackManager _ambushAttackManager;
        [SerializeField] private GameObject _roarEffect;
        [SerializeField] private Transform _dropAttack_roarEffectPosition;
        [Space]
        [SerializeField] private Weapon _ambushWeapon;
        [SerializeField] private float _ambushWeaponActiveDuration;

        [Serializable]
        private class AnimationTimeScale
        {
            public string AnimationName;
            public float TimeScale = 1f;

            public AnimationTimeScale(string animationName, float timeScale = 1f)
            {
                AnimationName = animationName;
                TimeScale = timeScale;
            }
        };

        [Header("Animation")]
        [SerializeField] private AnimationTimeScale[] _animationTimeScales = new AnimationTimeScale[]
        {
            new("BiteAttack", 1.0f),
            new("DropAttack", 1.0f),
            new("AmbushAttack", 1.0f),
        };

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        internal override GameObject DetectedPlayer => _player?.gameObject;
        private const string IsAwake = nameof(IsAwake);


        // States
        private class CerberusBrain : MonsterBrain
        {
            public CerberusBrain(IMonsterInternal owner) : base(owner)
            {
                Blackboard.Properties[IsAwake] = false;

                AddChild(new Alive()
                    .AddChild(new Idle(IsAwake))
                    .AddChild(new Awaken(IsAwake)
                        {
                            HierarchyMode = HierarchyMode.Sequence,
                            LoopType = LoopType.None,
                        }
                        .AddChild(new Attack("AmbushAttack_Intro"))
                        .AddChild(new CerberusAttackPhaseBrain()
                            .AddChild(new CerberusAttackBrain())
                            .AddChild(new Await(
                                MonsterActionType.Idle,
                                Owner.StatsInfo.AttackCooltime
                            ))
                        )
                    )
                );
                AddChild(new Dead() { DestroyOwnerOnCompleted = false });
            }
        }

        private class CerberusActionController : MonsterActionController
        {
            public CerberusActionController(Cerberus cerberus) : base(cerberus)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());

                #region Attacks
                AddChild(new MonsterAction("BiteAttack")
                    .AddComponent(new SetTimeScale(cerberus
                        ._animationTimeScales
                        .FirstOrDefault(ats => ats.AnimationName == "BiteAttack")
                        ?.TimeScale ?? 1f)
                    )
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(
                        cerberus._biteWeapon,
                        cerberus._biteWeaponActiveTiming,
                        cerberus._biteWeaponActiveDuration
                    ))

                    //.AddDelay(0.5f, out var bite_delay)
                    //.AddComponent(new Do(true, () =>
                    //{
                    //    var effect = Instantiate(cerberus._roarEffect);
                    //    effect.transform.position = cerberus._roarEffect.transform.position;
                    //    effect.SetActive(true);

                    //    Destroy(effect, 5f);
                    //}), after: new(bite_delay))
                    //.AddDelay(0.5f, after: new(bite_delay))
                );

                AddChild(new MonsterAction("DropAttack")
                    .AddComponent(new SetTimeScale(cerberus
                        ._animationTimeScales
                        .FirstOrDefault(ats => ats.AnimationName == "DropAttack")
                        ?.TimeScale ?? 1f)
                    )
                    .AddAnimationComponent("Roar", out var dropAttack_roar)
                    .AddDelay(1.3f, out var drop_delay)
                    .AddComponent(new Do(true, () =>
                        {
                            var effect = Instantiate(cerberus._roarEffect);
                            effect.transform.position = cerberus._dropAttack_roarEffectPosition.position;
                            effect.SetActive(true);

                            Destroy(effect, 5f);
                        }),
                        after: new(drop_delay)
                    )
                    .AddAnimationComponent("Idle", after: new(dropAttack_roar))
                    .AddComponent(new Do(false)
                        .AssignTo(out var roar_doFall)
                        .OnOpening(() => cerberus._fallingStoneManager.DoFall(succeed =>
                            roar_doFall.Interrupt(succeed ? InterruptType.Completed : InterruptType.Error))
                        ),
                        after: new(dropAttack_roar)
                    )
                    .AddDelay(
                        0.5f,
                        after: new(roar_doFall),
                        interruptPriority: InterruptPriority.High
                    )
                );

                AddChild(new MonsterAction("AmbushAttack_Intro")
                    .AddComponent(new SetTimeScale(cerberus
                        ._animationTimeScales
                        .FirstOrDefault(ats => ats.AnimationName == "AmbushAttack")
                        ?.TimeScale ?? 1f)
                    )
                    .AddAnimationComponent(
                        "AmbushAttack",
                        out var ambushIntro_anim
                    )
                    .AddDelay(
                        0.4f,
                        out var ambushIntro_attack
                    )
                    .AddComponent(
                        new Do(true, () => cerberus._ambushAttackManager.ShowSmokeEffect()),
                        after: new(ambushIntro_attack)
                    )
                    .AddComponent(new AttackWithWeapon(
                        cerberus._ambushWeapon,
                        0,
                        cerberus._ambushWeaponActiveDuration),
                        after: new(ambushIntro_attack)
                    )
                    .AddAnimationComponent(
                        "Idle",
                        after: new(ambushIntro_anim)
                    )
                    .AddDelay(
                        1f,
                        after: new(ambushIntro_attack),
                        interruptPriority: InterruptPriority.High
                    )
                );

                AddChild(new MonsterAction("AmbushAttack")
                    .AddComponent(new SetTimeScale(cerberus
                        ._animationTimeScales
                        .FirstOrDefault(ats => ats.AnimationName == "AmbushAttack")
                        ?.TimeScale ?? 1f)
                    )
                    .AddComponent(new Do(true, () => cerberus._ambushAttackManager.ShowIndicator()))
                    .AddDelay(
                        2f,
                        out var ambush_showIndicator
                    )
                    .AddAnimationComponent(
                        out var ambush_attack_anim,
                        after: new(ambush_showIndicator)
                    )
                    .AddDelay(
                        0.4f,
                        out var ambush_attack, 
                        after: new(ambush_showIndicator)
                    )
                    .AddComponent(
                        new Do(true, () => cerberus._ambushAttackManager.ShowSmokeEffect()),
                        after: new(ambush_attack)
                    )
                    .AddComponent(new AttackWithWeapon(
                        cerberus._ambushWeapon,
                        0,
                        cerberus._ambushWeaponActiveDuration),
                        after: new(ambush_attack)
                    )
                    .AddAnimationComponent(
                        "Idle",
                        after: new(ambush_attack_anim)
                    )
                    .AddDelay(
                        3f,
                        after: new(ambush_attack),
                        interruptPriority: InterruptPriority.High
                    )
                );
                #endregion

                AddChild(new MonsterAction(MonsterActionType.Hit)
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

        protected override void Awake()
        {
            if (!_fallingStoneManager)
                throw new InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_fallingStoneManager)}'을(를) 가지고 있어야 합니다.");

            if (!_ambushAttackManager)
                throw new InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_ambushAttackManager)}'을(를) 가지고 있어야 합니다.");

            if (!_roarEffect)
                throw new InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_roarEffect)}'을(를) 가지고 있어야 합니다.");

            if (!_dropAttack_roarEffectPosition)
                throw new InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_dropAttack_roarEffectPosition)}'을(를) 가지고 있어야 합니다.");

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();

            if (_biteWeapon)
            {
                _biteWeapon.AttackPower = StatsInfo.BiteAttackPower;
                _biteWeapon.gameObject.SetActive(false);
            }
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_biteWeapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            if (_ambushWeapon)
            {
                _ambushWeapon.AttackPower = StatsInfo.AmbushAttackPower;
                _ambushWeapon.gameObject.SetActive(false);
            }
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_ambushWeapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            _fallingStoneManager
                .Initialize(Configuration, PlatformManager)
                .SetProjectileInitializer(
                    stone => stone.GetComponent<Weapon>().AttackPower = StatsInfo.DropAreaAttackPower);

            ActionController = new CerberusActionController(this);
            ActionController.Enter();

            StandaloneHitBrain.DoKnockback = false;
            Brain = new CerberusBrain(this);


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
            message += $"\nAwaken: {Brain.Blackboard.Properties[IsAwake]}";

            return message;
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(Cerberus))]
        private class CerberusEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (Cerberus)base.target;

                if (!target._autoAwake && GUILayout.Button("Awake"))
                    target.DoAwake();
            }
        }
#endif
    }
}
