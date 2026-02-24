using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Infrastructure;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace World
{
    public class PlatformManager : MonoBehaviour
    {
        // Front
        /// <summary>
        /// 입력 좌표에 대한 플랫폼 ID 정보
        /// </summary>
        public IReadOnlyDictionary<Vector3Int, int> Platforms { get; private set; }
        /// <summary>
        /// 타일맵의 Cell 사이즈
        /// </summary>
        public Vector3 CellSize { get; private set; }
        /// <summary>
        /// 모든 타일맵의 경계 (단위: World Position)
        /// </summary>
        public Bounds Bounds { get; private set; }
        public Tilemap OneWayPlatformTilemap
        {
            get
            {
                if (_tilemaps == null || _tilemaps.Length < 2)
                    return null;
                else return _tilemaps[1];
            }
        }
        public List<int> OneWayPlatformIds => _oneWayPlatformIds;

        // Property
        [SerializeField] private bool _autoAssignTilemaps = true;
        [SerializeField] private Tilemap[] _tilemaps;
        [Space]
        [SerializeField] private int _platformCount = 0;
        [SerializeField] private bool _enableDebugger = false;

        // Internal
        private Dictionary<Vector3Int, int> _platforms;
        private BoundsInt _cellBounds;
        private bool _initialized = false;
        private List<int> _oneWayPlatformIds = new();

        private readonly Dictionary<int, Color> _debuggerColormap = new();


        // Content
        private void Awake()
        {
            _platforms = new();
            Platforms = new ReadOnlyDictionary<Vector3Int, int>(_platforms);

            if (_autoAssignTilemaps)
                _tilemaps = GetComponentsInChildren<Tilemap>();

            _initialized = true;
            SetPlatforms();
        }

        /// <summary>
        /// 플랫폼을 생성하려면 이 메서드를 호출하세요.
        /// </summary>
        public void SetPlatforms()
        {
            ThrowIfNotInitialized();

            _platforms.Clear();
            _debuggerColormap.Clear();
            CellSize = default;
            _cellBounds = default;

            if (_tilemaps == null || _tilemaps.Length == 0)
            {
                Debug.LogWarning(
                    $"[{nameof(SetPlatforms)}] {nameof(_tilemaps)}이(가) 할당되지 않았기 때문에 타일맵이 생성되지 않았습니다.", this);

                return;
            }

            var cellSize = _tilemaps[0].cellSize;

            foreach (var tilemap in _tilemaps)
            {
                if ((Vector2)tilemap.transform.position != Vector2.zero)
                    throw new InvalidOperationException(
                        $"[{nameof(SetPlatforms)}] 타일맵 '{tilemap.name}'의 위치가 {(Vector2)tilemap.transform.position}입니다. " +
                        $"모든 타일맵의 위치는 (0, 0)이어야 합니다.");

                if (tilemap.cellSize != cellSize)
                    throw new InvalidOperationException(
                        $"[{nameof(SetPlatforms)}] 타일맵 '{tilemap.name}'의 {nameof(Tilemap.cellSize)}이(가) {tilemap.cellSize}입니다. " +
                        $"모든 타일맵의 {nameof(Tilemap.cellSize)}은(는) {cellSize}(이)여야 합니다.");
            }

            CellSize = cellSize;


            int id = 1;
            bool isFirst = true;
            int index = 0;

            foreach (var tilemap in _tilemaps)
            {
                tilemap.CompressBounds();

                if (isFirst)
                {
                    _cellBounds = tilemap.cellBounds;
                    isFirst = false;
                }
                else
                {
                    _cellBounds.SetMinMax(
                        Vector3Int.Min(_cellBounds.min, tilemap.cellBounds.min),
                        Vector3Int.Max(_cellBounds.max, tilemap.cellBounds.max));
                }

                foreach (var cell in tilemap.cellBounds.allPositionsWithin)
                {
                    if (!tilemap.HasTile(cell))
                        continue;
                    if (_platforms.ContainsKey(cell))
                        continue;


                    var predicate = GetPredicate(cell);

                    if (!tilemap.TryGetPlatform(cell, predicate, out var platformCells))
                        continue;

                    foreach (var platformCell in platformCells)
                        _platforms.Add(platformCell, id);

                    if (index == 1)
                        _oneWayPlatformIds.Add(id);

                    id++;
                }

                index++;
            }

            _platformCount = id - 1;


            var bounds = new Bounds();

            bounds.SetMinMax(
                _tilemaps[0].CellToWorld(_cellBounds.min),
                _tilemaps[0].CellToWorld(_cellBounds.max));

            bounds.size = new(bounds.size.x, bounds.size.y, 1);

            Bounds = bounds;
        }

        /// <summary>
        /// 이 메서드를 재정의하여 플랫폼 구분 규칙을 설정하세요.
        /// </summary>
        protected virtual Func<Vector3Int, bool> GetPredicate(Vector3Int criteria)
        {
            return cell => true;
        }

        /// <summary>
        /// 월드 좌표를 입력하면 해당 좌표에 존재하는 플랫폼의 id를 반환합니다.
        /// </summary>
        public int GetPlatformId(Vector2 position)
        {
            ThrowIfNotInitialized();
            return GetPlatformId(_tilemaps[0].WorldToCell(position));
        }
        /// <summary>
        /// 타일 좌표를 입력하면 해당 좌표에 존재하는 플랫폼의 id를 반환합니다.
        /// </summary>
        public int GetPlatformId(Vector3Int cell)
        {
            ThrowIfNotInitialized();

            if (!TryGetPlatformId(cell, out var id))
                throw new ArgumentException($"입력한 좌표({cell})에 해당하는 플랫폼을 찾는 데 실패했습니다.", nameof(cell));

            return id;
        }

        /// <summary>
        /// 입력한 월드 좌표에 플랫폼이 존재한다면 해당 플랫폼의 id를 반환합니다.
        /// </summary>
        /// <param name="id">입력한 월드 좌표에 해당하는 플랫폼 id</param>
        /// <returns>플랫폼이 존재하면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool TryGetPlatformId(Vector2 position, out int id)
        {
            ThrowIfNotInitialized();
            return TryGetPlatformId(_tilemaps[0].WorldToCell(position), out id);
        }
        /// <summary>
        /// 입력한 타일 좌표에 플랫폼이 존재한다면 해당 플랫폼의 id를 반환합니다.
        /// </summary>
        /// <param name="id">입력한 타일 좌표에 해당하는 플랫폼 id</param>
        /// <returns>플랫폼이 존재하면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public bool TryGetPlatformId(Vector3Int cell, out int id)
        {
            ThrowIfNotInitialized();

            if (!_cellBounds.Contains(cell))
            {
                id = -1;
                return false;
            }

            return _platforms.TryGetValue(cell, out id);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!_enableDebugger || !_initialized)
                return;

            foreach (var (cell, id) in _platforms)
            {
                var center = _tilemaps[0].GetCellCenterWorld(cell);
                var size = CellSize;

                if (!_debuggerColormap.TryGetValue(id, out var color))
                {
                    color = UnityEngine.Random.ColorHSV(
                        0f, 1f,    // Hue 범위 (0~1)
                        0.8f, 1f,  // Saturation 범위
                        0.6f, 1f,  // Value 범위
                        1f, 1f     // Alpha 범위
                    );

                    _debuggerColormap[id] = color;
                }

                Gizmos.color = color;
                Gizmos.DrawWireCube(center, size);
            }
        }
#endif

        private void ThrowIfNotInitialized()
        {
            if (!_initialized)
                throw new InvalidOperationException(
                    $"{nameof(PlatformManager)}이(가) 초기화되지 않았기 때문에 작업을 수행할 수 없습니다.");
        }
    }
}
