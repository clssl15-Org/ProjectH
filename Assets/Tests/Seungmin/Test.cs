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
