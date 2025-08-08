using Infrastructure;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Tests.Seungmin
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private Vector2Int coord;


        [CustomEditor(typeof(Test))]
        private class CubeGenerateButton : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (Test)base.target;

                if (GUILayout.Button("Get platform"))
                {
                    if (!target.tilemap)
                        return;

                    var coord = new Vector3Int(target.coord.x, target.coord.y, 0);

                    if (target.tilemap.TryGetPlatform(
                        coord, t => target.tilemap.GetTile(t).name == "Tileset1_37", out var coords))
                    {
                        foreach (var tile in coords)
                            print($"{tile}, {target.tilemap.GetSprite(tile)}");
                    }
                    else
                    {
                        print("Failed to get platform");
                    }
                }
            }
        }
    }
}
