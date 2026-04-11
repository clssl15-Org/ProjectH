#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

namespace Tests.Seungmin
{
    public class Test : MonoBehaviour
    {

        private void Start()
        {

        }

        private void Update()
        {

        }

        public void DoTest()
        {
            
        }


#if UNITY_EDITOR
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
#endif
    }
}
