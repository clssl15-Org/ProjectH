using Infrastructure;
using TMPro;
using UnityEngine;

namespace Actors.Monsters
{
    internal static partial class MonsterTools
    {
        public static GameObject ShowAsIndicator(
            this GameObject indicator,
            IMonsterInternal owner,
            float height = 0.3f,
            float showTime = 0.5f)
        {
            indicator.transform.SetParent(owner.transform);
            indicator.transform.position = new Vector2
            {
                x = owner.transform.position.x,
                y = owner.Collider.bounds.max.y + height
            };

            if (indicator.TryGetComponent<SpriteSizeHandler>(out var ssh))
                ssh.Initialize(owner.Configuration, true);

            Object.Destroy(indicator, showTime);

            return indicator;
        }

        public static TextMeshPro ShowAsIndicator(
            this TextMeshPro indicator,
            IMonsterInternal owner,
            string text,
            float height = 0.7f,
            float showTime = 0.5f,
            float fontSize = 5f)
        {
            ShowAsIndicator(indicator.gameObject, owner, height, showTime);

            indicator.text = text;
            indicator.fontSize = fontSize;

            return indicator;
        }
    }
}
