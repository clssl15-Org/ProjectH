using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public static partial class Tools
    {
        public static T GetRandomItem<T>(this IList<T> items)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            return items[UnityEngine.Random.Range(0, items.Count)];
        }

        public static T GetRandomItem<T>(this IList<T> items, ref int previousIndex)
        {
            if (items is null)
                throw new ArgumentNullException(nameof(items));

            if (items.Count == 0)
            {
                previousIndex = -1;
                return default;
            }

            var currentIndex = 0;
            if (items.Count > 1)
            {
                do
                {
                    currentIndex = UnityEngine.Random.Range(0, items.Count);
                } while (currentIndex == previousIndex);
            }

            previousIndex = currentIndex;
            return items[currentIndex];
        }
    }
}
