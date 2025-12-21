using Actors.Monsters.Actions;
using Actors.Monsters.Brains;
using Infrastructure.StateMachines.BT;
using UnityEngine;

namespace Actors.Monsters
{
    [RequireComponent(typeof(StandaloneHitAction))]
    public class EyeballMonsterFirstPhase : Monster<MonsterStats>
    {
        // Property
        [Header("Eyeball Monster First Phase")]
        [SerializeField] private Weapon _weapon;
        [SerializeField, Min(0)] private float _weaponActiveTiming;
        [SerializeField] private float _weaponActiveDuration;
        [Space]
        [SerializeField] private bool _revive = true;
        [SerializeField] private GameObject[] _secondPhasePrefabs;


        // Internal
        private class EyeballMonsterFirstPhaseBrain : MonsterBrain
        {
            public EyeballMonsterFirstPhaseBrain(EyeballMonsterFirstPhase owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(
                                    Engaged.RangeType.Ranged,
                                    Mathf.Abs(
                                        owner._weapon?.transform.localPosition.x
                                        ?? Engaged.DefaultTargetAttackRange)
                                )
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd())
                            )
                            .AddChild(new Attack())
                            .AddChild(new Cooldown())
                        )
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol())
                        )
                    )
                    .AddChild(new NotValidPlatform())
                );
                AddChild(new Dead());
            }
        }

        private class EyeballMonsterFirstPhaseController : MonsterActionController
        {
            public EyeballMonsterFirstPhaseController(EyeballMonsterFirstPhase monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent()
                );
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent(interruptPriority: InterruptPriority.High)
                    .AddComponent(new AttackWithWeapon(
                        monster._weapon,
                        monster._weaponActiveTiming,
                        monster._weaponActiveDuration))
                );
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelay()
                    .AddComponent(new HitFlash())
                );
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent()
                );
            }
        }


        // Content
        protected override void Start()
        {
            base.Start();

            if (_weapon)
                _weapon.AttackPower = StatsInfo.AttackPower;
            else
                Debug.LogWarning(
                    FormatLogMessage($"{nameof(_weapon)}이(가) 등록되지 않았으므로 공격력 설정이 반영되지 않았습니다."),
                    this);

            ActionController = new EyeballMonsterFirstPhaseController(this);
            ActionController.Enter();

            Brain = new EyeballMonsterFirstPhaseBrain(this);
        }

        internal override void Die()
        {
            if (_revive)
            {
                foreach (var secondPrefab in _secondPhasePrefabs)
                {
                    if (!secondPrefab)
                    {
                        Debug.LogWarning(FormatLogMessage(
                            $"{nameof(secondPrefab)}이(가) 유효하지 않기 때문에 등록된 몬스터 중 일부가 생성되지 않습니다."));

                        continue;
                    }

                    var second = Instantiate(secondPrefab);
                    second
                        .GetComponent<EyeballMonsterSecondPhase>()
                        .Initialize(GameAssetsLibrary, Configuration, PlatformManager);

                    second.transform.position = transform.position;
                    second.SetActive(true);
                }
            }

            base.Die();
        }

        protected override void OnDamaged(DamageInfo damageInfo)
        {
            Brain.SelectChild(new SelectionRequest[]
            {
                new(true),
                new(true),
                new(nameof(Hit), new object[] { damageInfo }, EntryPolicy.CheckAlways, RerunPolicy.Restart)
            });
        }
    }
}
