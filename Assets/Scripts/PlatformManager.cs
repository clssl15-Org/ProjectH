using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;
using UnityEngine.Tilemaps;
using Infrastructure;

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
    public Vector3 CellSize
    {
        get
        {
            ThrowIfNotInitialized();
            return _tilemap.cellSize;
        }
    }

    public Bounds Bound => _tilemap.localBounds;

    // Property
    [SerializeField] private Tilemap _tilemap;
    [SerializeField] private bool _enableDebugger = false;

    // Internal
    private Dictionary<Vector3Int, int> _platforms;
    private bool _initialized = false;

    private readonly Dictionary<int, Color> _debuggerColormap = new();
    

    // Content
    private void Awake()
    {
        _platforms = new();
        Platforms = new ReadOnlyDictionary<Vector3Int, int>(_platforms);

        _initialized = true;
        SetPlatforms();
    }

    /// <summary>
    /// 플랫폼을 생성하려면 이 메서드를 호출하세요.
    /// </summary>
    public void SetPlatforms()
    {
        ThrowIfNotInitialized();

        if (!_tilemap)
            throw new InvalidOperationException(
                $"{nameof(_tilemap)}이(가) 할당되지 않았기 때문에 {nameof(SetPlatforms)} 메서드를 실행할 수 없습니다.");

        _platforms.Clear();
        _debuggerColormap.Clear();

        _tilemap.CompressBounds();


        int id = 1;

        foreach (var cell in _tilemap.cellBounds.allPositionsWithin)
        {
            if (!_tilemap.HasTile(cell))
                continue;
            if (_platforms.ContainsKey(cell))
                continue;

            var predicate = GetPredicate(cell);

            if (!_tilemap.TryGetPlatform(cell, predicate, out var platformCells))
                continue;


            foreach (var platformCell in platformCells)
                _platforms.Add(platformCell, id);

            id++;
        }
    }

    /// <summary>
    /// 이 메서드를 재정의하여 플랫폼 구분 규칙을 설정하세요.
    /// </summary>
    protected virtual Predicate<Vector3Int> GetPredicate(Vector3Int criteria)
    {
        return cell => true;
    }

    /// <summary>
    /// 월드 좌표를 입력하면 해당 좌표에 존재하는 플랫폼의 id를 반환합니다.
    /// </summary>
    public int GetPlatformId(Vector2 position)
    {
        ThrowIfNotInitialized();
        return GetPlatformId(_tilemap.WorldToCell(position));
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
        return TryGetPlatformId(_tilemap.WorldToCell(position), out id);
    }
    /// <summary>
    /// 입력한 타일 좌표에 플랫폼이 존재한다면 해당 플랫폼의 id를 반환합니다.
    /// </summary>
    /// <param name="id">입력한 타일 좌표에 해당하는 플랫폼 id</param>
    /// <returns>플랫폼이 존재하면 true, 그렇지 않으면 false를 반환합니다.</returns>
    public bool TryGetPlatformId(Vector3Int cell, out int id)
    {
        ThrowIfNotInitialized();
        return _platforms.TryGetValue(cell, out id);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_enableDebugger || !_initialized || !_tilemap)
            return;

        foreach (var (cell, id) in _platforms)
        {
            var center = _tilemap.GetCellCenterWorld(cell);
            var size = _tilemap.cellSize;

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
