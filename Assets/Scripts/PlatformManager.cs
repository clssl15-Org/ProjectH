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
            return tilemap.cellSize;
        }
    }

    // Property
    [SerializeField] private Tilemap tilemap;
    [SerializeField] private bool enableDebugger = false;

    // Internal
    private Dictionary<Vector3Int, int> platforms;
    private bool initialized = false;

    private readonly Dictionary<int, Color> debuggerColormap = new();
    

    // Content
    private void Awake()
    {
        platforms = new();
        Platforms = new ReadOnlyDictionary<Vector3Int, int>(platforms);

        initialized = true;
        SetPlatforms();
    }

    /// <summary>
    /// 플랫폼을 생성하려면 이 메서드를 호출하세요.
    /// </summary>
    public void SetPlatforms()
    {
        ThrowIfNotInitialized();
        if (!tilemap) throw new InvalidOperationException("Tilemap이 할당되지 않았기 때문에 SetPlatforms 메서드를 실행할 수 없습니다.");

        platforms.Clear();
        debuggerColormap.Clear();

        tilemap.CompressBounds();


        int id = 1;

        foreach (var cell in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(cell))
                continue;
            if (platforms.ContainsKey(cell))
                continue;

            var predicate = GetPredicate(cell);

            if (!tilemap.TryGetPlatform(cell, predicate, out var platformCells))
                continue;


            foreach (var platformCell in platformCells)
                platforms.Add(platformCell, id);

            id++;
        }
    }

    // 여기에서 플랫폼 구분 규칙을 설정하세요.
    private Predicate<Vector3Int> GetPredicate(Vector3Int criteria)
    {
        return cell => true;
    }

    /// <summary>
    /// 월드 좌표를 입력하면 해당 좌표에 존재하는 플랫폼의 id를 반환합니다.
    /// </summary>
    public int GetPlatformId(Vector2 position)
    {
        ThrowIfNotInitialized();
        return GetPlatformId(tilemap.WorldToCell(position));
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
        return TryGetPlatformId(tilemap.WorldToCell(position), out id);
    }
    /// <summary>
    /// 입력한 타일 좌표에 플랫폼이 존재한다면 해당 플랫폼의 id를 반환합니다.
    /// </summary>
    /// <param name="id">입력한 타일 좌표에 해당하는 플랫폼 id</param>
    /// <returns>플랫폼이 존재하면 true, 그렇지 않으면 false를 반환합니다.</returns>
    public bool TryGetPlatformId(Vector3Int cell, out int id)
    {
        ThrowIfNotInitialized();
        return platforms.TryGetValue(cell, out id);
    }

    private void OnDrawGizmos()
    {
        if (!enableDebugger || !initialized || !tilemap)
            return;

        foreach (var (cell, id) in platforms)
        {
            var center = tilemap.GetCellCenterWorld(cell);
            var size = tilemap.cellSize;

            if (!debuggerColormap.TryGetValue(id, out var color))
            {
                color = UnityEngine.Random.ColorHSV(
                    0f, 1f,    // Hue 범위 (0~1)
                    0.8f, 1f,  // Saturation 범위
                    0.6f, 1f,  // Value 범위
                    1f, 1f     // Alpha 범위
                    );

                debuggerColormap[id] = color;
            }

            Gizmos.color = color;
            Gizmos.DrawWireCube(center, size);
        }
    }

    private void ThrowIfNotInitialized()
    {
        if (!initialized)
            throw new InvalidOperationException("Platform Manager가 초기화되지 않았기 때문에 작업을 수행할 수 없습니다.");
    }
}
