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
        [SerializeField] private FallingStoneManager _fallingStoneManager;
        [SerializeField] private AmbushAttackManager _ambushAttackManager;
        [SerializeField] private GameObject _roarEffect;
        [SerializeField] private Transform _dropAttack_roarEffectPosition;

        [Header("Debug")]
        [SerializeField] private bool _useTargetPlayer = false;
        [SerializeField] private GameObject _targetPlayer;
        [Space]
        [SerializeField] private bool _autoAwake = false;

        internal override GameObject DetectedPlayer => _player?.gameObject;
        private const string IsAwake = nameof(IsAwake);


        // Internal
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
                    .AddAnimationComponent()
                    .AddDelay(0.5f, out var bite_delay)
                    .AddComponent(new Do(true, () =>
                    {
                        var effect = Instantiate(cerberus._roarEffect);
                        effect.transform.position = cerberus._roarEffect.transform.position;
                        effect.SetActive(true);

                        Destroy(effect, 5f);
                    }), after: new(bite_delay))
                    .AddDelay(0.5f, after: new(bite_delay))
                );

                AddChild(new MonsterAction("DropAttack")
                    .AddAnimationComponent("Roar", out var dropAttack_roar)
                    .AddDelay(1.3f, out var drop_delay)
                    .AddComponent(new Do(true, () =>
                    {
                        var effect = Instantiate(cerberus._roarEffect);
                        effect.transform.position = cerberus._dropAttack_roarEffectPosition.position;
                        effect.SetActive(true);

                        Destroy(effect, 5f);
                    }), after: new(drop_delay))
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
                        interruptAllOnDeactivate: true
                    )
                );

                AddChild(new MonsterAction("AmbushAttack_Intro")
                    .AddAnimationComponent("AmbushAttack")
                    .AddDelay(
                        0.4f,
                        out var ambushIntro_attack
                    )
                    .AddComponent(
                        new Do(true, () => cerberus._ambushAttackManager.ShowSmokeEffect()),
                        after: new(ambushIntro_attack)
                    )
                    .AddAnimationComponent("Idle", after: new(ambushIntro_attack))
                    .AddDelay(
                        1f,
                        after: new(ambushIntro_attack),
                        interruptAllOnDeactivate: true
                    )
                );

                AddChild(new MonsterAction("AmbushAttack")
                    .AddComponent(new Do(true, () => cerberus._ambushAttackManager.ShowIndicator()))
                    .AddDelay(
                        2f,
                        out var ambush_showIndicator
                    )
                    .AddAnimationComponent(after: new(ambush_showIndicator))
                    .AddDelay(
                        0.4f,
                        out var ambush_attack, 
                        after: new(ambush_showIndicator)
                    )
                    .AddComponent(
                        new Do(true, () => cerberus._ambushAttackManager.ShowSmokeEffect()),
                        after: new(ambush_attack)
                    )
                    .AddAnimationComponent("Idle", after: new(ambush_attack))
                    .AddDelay(
                        3f,
                        after: new(ambush_attack),
                        interruptAllOnDeactivate: true
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
                throw new System.InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_fallingStoneManager)}'을(를) 가지고 있어야 합니다.");

            if (!_ambushAttackManager)
                throw new System.InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_ambushAttackManager)}'을(를) 가지고 있어야 합니다.");

            if (!_roarEffect)
                throw new System.InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_roarEffect)}'을(를) 가지고 있어야 합니다.");

            if (!_dropAttack_roarEffectPosition)
                throw new System.InvalidOperationException(
                    $"{nameof(Cerberus)}은(는) '{nameof(_dropAttack_roarEffectPosition)}'을(를) 가지고 있어야 합니다.");

            base.Awake();
        }

        protected override void Start()
        {
            base.Start();
            _fallingStoneManager.Initialize(Configuration, PlatformManager);

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
