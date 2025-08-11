using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Infrastructure
{
    public static partial class Tools
    {
        /// <summary>
        /// 입력 Tilemap에서 start 좌표를 기준으로, predicate를 만족하며 수평으로 연결된 타일들을 반환합니다.
        /// </summary>
        public static bool TryGetPlatform(this Tilemap tilemap, Vector3Int start, Predicate<Vector3Int> predicate, out Vector3Int[] platformCells)
        {
            platformCells = Array.Empty<Vector3Int>();

            if (tilemap == null)
                throw new ArgumentNullException(nameof(tilemap), "입력 Tilemap이 null입니다.");

            var bounds = tilemap.cellBounds;

            if (!bounds.Contains(start))
                return false;
            if (!tilemap.HasTile(start))
                return false;

            var y = start.y;
            var z = start.z;

            if (!IsValidX(start.x))
                return false;


            // 왼쪽 끝 찾기
            var leftX = start.x;
            while (IsValidX(leftX - 1)) leftX--;

            // 오른쪽 끝 찾기
            var rightX = start.x;
            while (IsValidX(rightX + 1)) rightX++;

            bool IsValidX(int x)
            {
                var current = new Vector3Int(x, y, z);

                return bounds.Contains(current)
                    && tilemap.HasTile(current)
                    && !tilemap.HasTile(current + Vector3Int.up)
                    && (predicate?.Invoke(current) ?? true);
            }


            platformCells = new Vector3Int[rightX - leftX + 1];

            for (int i = 0; i < platformCells.Length; i++)
                platformCells[i] = new Vector3Int(leftX++, y, z);

            return true;
        }
    }
}
