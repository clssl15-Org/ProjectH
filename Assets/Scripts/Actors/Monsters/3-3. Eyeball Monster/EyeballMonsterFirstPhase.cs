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
        [SerializeField] private bool _revive = true;
        [SerializeField] private GameObject[] _secondPhasePrefabs;


        // Internal
        private class DarkMonsterFirstPhaseBrain : MonsterBrain
        {
            public DarkMonsterFirstPhaseBrain(IMonsterInternal owner) : base(owner)
            {
                AddChild(new Alive()
                    .AddChild(new Hit())
                    .AddChild(new ValidPlatform()
                        .AddChild(new PlayerDetected()
                            .AddChild(new Engaged(Engaged.RangeType.Contact, range: 1)
                                .AddChild(new Adjusting(MonsterActionType.Walk))
                                .AddChild(new DeadEnd()))
                            .AddChild(new Attack())
                            .AddChild(new Cooldown()))
                        .AddChild(new PlayerNotDetected()
                            .AddChild(new Rest())
                            .AddChild(new Patrol())))
                    .AddChild(new NotValidPlatform()));
                AddChild(new Dead());
            }
        }

        private class DarkMonsterFirstPhaseController : MonsterActionController
        {
            public DarkMonsterFirstPhaseController(IMonsterInternal monster) : base(monster)
            {
                AddChild(new MonsterAction(MonsterActionType.Idle)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Walk)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Attack)
                    .AddAnimationComponent());
                AddChild(new MonsterAction(MonsterActionType.Hit)
                    .AddDelayComponent()
                    .AddComponent(new HitFlash()));
                AddChild(new MonsterAction(MonsterActionType.Dead)
                    .AddAnimationComponent());
            }
        }


        // Content
        protected override void Start()
        {
            base.Start();

            ActionController = new DarkMonsterFirstPhaseController(this);
            ActionController.Enter();

            Brain = new DarkMonsterFirstPhaseBrain(this);
        }

        internal override void Died()
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
                        .Initialize(GameAssetsLibrary, PlatformManager);

                    second.transform.position = transform.position;
                    second.SetActive(true);
                }
            }

            base.Died();
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
