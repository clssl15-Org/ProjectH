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

    // Property
    [SerializeField] private Tilemap tilemap;

    // Internal
    private Dictionary<Vector3Int, int> platforms;
    private bool initialized = false;
    

    // Content
    private void Awake()
    {
        platforms = new();
        Platforms = new ReadOnlyDictionary<Vector3Int, int>(platforms);

        initialized = true;
    }

    /// <summary>
    /// 플랫폼을 생성하려면 이 메서드를 호출하세요.
    /// </summary>
    public void SetPlatforms(Predicate<Vector3Int> predicate)
    {
        ThrowIfNotInitialized();
        if (!tilemap) throw new InvalidOperationException("Tilemap이 할당되지 않았기 때문에 SetPlatforms 메서드를 실행할 수 없습니다.");

        platforms.Clear();
        tilemap.CompressBounds();

        int id = 0;

        foreach (var coord in tilemap.cellBounds.allPositionsWithin)
        {
            if (!tilemap.HasTile(coord))
                continue;
            if (platforms.ContainsKey(coord))
                continue;
            if (!tilemap.TryGetPlatform(coord, predicate, out var units))
                continue;

            foreach (var unit in units)
                platforms.Add(unit, id);

            id++;
        }
    }

    /// <summary>
    /// 타일 좌표를 입력하면 해당 좌표에 존재하는 플랫폼의 id를 반환합니다.
    /// </summary>
    public int GetPlatformID(Vector3Int coord)
    {
        ThrowIfNotInitialized();

        if (!TryGetPlatformID(coord, out var id))
            throw new ArgumentException($"입력한 좌표에 해당하는 플랫폼을 찾는 데 실패했습니다.", nameof(coord));

        return id;
    }
    /// <summary>
    /// 입력한 타일 좌표에 플랫폼이 존재한다면 해당 플랫폼의 id를 반환합니다.
    /// </summary>
    /// <param name="id">입력한 타일 좌표에 해당하는 플랫폼 id</param>
    /// <returns>플랫폼이 존재하면 true, 그렇지 않으면 false를 반환합니다.</returns>
    public bool TryGetPlatformID(Vector3Int coord, out int id)
    {
        ThrowIfNotInitialized();
        return platforms.TryGetValue(coord, out id);
    }


    private void ThrowIfNotInitialized()
    {
        if (!initialized)
            throw new InvalidOperationException("Platform Manager가 초기화되지 않았기 때문에 작업을 수행할 수 없습니다.");
    }
}
