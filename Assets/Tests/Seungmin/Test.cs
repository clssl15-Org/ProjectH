using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Infrastructure;

namespace Tests.Seungmin
{
    public class Test : MonoBehaviour
    {
        public void DoTest()
        {

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
