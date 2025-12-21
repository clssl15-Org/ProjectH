using UnityEngine;

namespace Actors.Monsters
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(IMonsterInternal))]
    public class IndicatorHub : MonoBehaviour
    {
        [field: Header("Player Detection")]
        [field: SerializeField] public bool ShowPlayerDetection { get; set; } = true;
        [field: SerializeField, Min(0)] public float PlayerDetectionHeight { get; set; } = 0.3f;
        [field: SerializeField, Min(0)] public float PlayerDetectionShowTime { get; set; } = 0.5f;

        [field: Header("Exclamation Mark")]
        [field: SerializeField] public bool ShowExclamationMark { get; set; } = true;
        [field: SerializeField, Min(0)] public float ExclamationMarkHeight { get; set; } = 0.3f;
        [field: SerializeField, Min(0)] public float ExclamationMarkShowTime { get; set; } = 0.5f;

        [field: Header("Damage Text")]
        [field: SerializeField] private bool ShowDamageText = true;
        [field: SerializeField, Min(0)] public float DamageFontSize { get; set; } = 5f;
        [field: SerializeField, Min(0)] public float DamageHeight { get; set; } = 0.7f;
        [field: SerializeField, Min(0)] public float DamageShowTime { get; set; } = 0.5f;

        private IMonsterInternal _monster;


        private void Start()
        {
            _monster = GetComponent<IMonsterInternal>();
            _monster.ConditionChanged += cd =>
            {
                if (cd.Is(MonsterCondition.PlayerDetected) && ShowPlayerDetection)
                    ShowPlayerDetectionIndicator();

                if (cd.Is(MonsterCondition.Attack) && ShowExclamationMark)
                {
                    if (cd.Payload is not bool isRanged)
                    {
                        Debug.LogWarning(
                            Ctx($"Attack 이벤트의 Payload는 bool 형식이어야 하지만 '{cd.Payload?.GetType().Name ?? "null"}'이(가) 감지되었습니다."),
                            this);
                        return;
                    }

                    if (!isRanged)
                        return;

                    ShowExclamationMarkIndicator();
                }

                if (cd.Is(MonsterCondition.Damaged) && ShowDamageText)
                {
                    if (cd.Payload is not DamageInfo damageInfo)
                    {
                        Debug.LogWarning(
                            Ctx($"Damaged 이벤트의 Payload는 {nameof(DamageInfo)} 형식이어야 하지만 '{cd.Payload?.GetType().Name ?? "null"}'이(가) 감지되었습니다."),
                            this);
                        return;
                    }

                    ShowDamageTextIndicator(damageInfo.Damage);
                }
            };
        }

        private void ShowPlayerDetectionIndicator()
        {
            _monster
                .GameAssetsLibrary
                .Indicator_PlayerDetection
                .ShowAsIndicator(
                    _monster,
                    PlayerDetectionHeight,
                    PlayerDetectionShowTime);
        }

        private void ShowExclamationMarkIndicator()
        {
            _monster
                .GameAssetsLibrary
                .Indicator_ExclamationMark
                .ShowAsIndicator(
                    _monster,
                    ExclamationMarkHeight,
                    ExclamationMarkShowTime);
        }

        private void ShowDamageTextIndicator(int damage)
        {
            _monster
                .GameAssetsLibrary
                .Indicator_Text
                .ShowAsIndicator(
                    _monster,
                    damage.ToString(),
                    DamageHeight,
                    DamageShowTime,
                    DamageFontSize);
        }


        private string Ctx(string message) => $"[{nameof(IndicatorHub)}] {message}";
    }
}
