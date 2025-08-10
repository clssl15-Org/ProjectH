using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Infrastructure;

namespace Tests.Seungmin
{
    public class Test : MonoBehaviour
    {
        [SerializeField] private PlatformManager platformManager;
        [SerializeField] private Tilemap tilemap;
        [SerializeField] private Vector3Int coord;


        private void DoTest()
        {
            if (!platformManager)
                return;

            print(platformManager.GetPlatformId(coord));
        }

        private void GetPlatform()
        {
            if (!tilemap)
                return;

            if (tilemap.TryGetPlatform(
                coord, t => tilemap.GetTile(t).name == "Tileset1_37", out var coords))
            {
                foreach (var tile in coords)
                    print($"{tile}, {tilemap.GetSprite(tile)}");
            }
            else
            {
                print("Failed to get platform");
            }
        }


        [CustomEditor(typeof(Test))]
        private class TestEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                var target = (Test)base.target;


                if (GUILayout.Button("Test"))
                {
                    target.DoTest();
                }
            }
        }
    }
}
