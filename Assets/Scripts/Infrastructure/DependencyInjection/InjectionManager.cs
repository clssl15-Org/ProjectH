using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure
{
    public class InjectionManager : MonoBehaviour
    {
        [SerializeField] private Injector[] _injectors;
        [SerializeField] private bool _injectAtAwake = false;

        private void Awake()
        {
            if (_injectAtAwake)
                Inject();
        }

        private void Inject()
        {
            foreach (var injector in _injectors)
                injector.Inject();
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(InjectionManager))]
        private class InjectionManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Inject All"))
                    ((InjectionManager)target).Inject();
            }
        }
#endif
    }
}
