using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure
{
    public class Injector : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour[] _injections;
        [SerializeField] private bool _injectAtAwake = false;

        private void Awake()
        {
            if (_injectAtAwake)
                Inject();
        }

        private void Inject()
        {
            foreach (var _injection in _injections)
                Inject(_injection);
        }

        private void Inject(MonoBehaviour injection)
        {
            if (!injection)
            {
                Debug.LogError($"[{nameof(Injector)}] {nameof(injection)}이(가) 유효하지 않습니다.", this);
                return;
            }

            var targetType = injection.GetType();
            var injectableInterfaceType = typeof(IInjectable<>).MakeGenericType(targetType);

            var injectMethod = injectableInterfaceType.GetMethod("Inject");
            if (injectMethod == null)
            {
                Debug.LogError($"IInjectable<{targetType.Name}> 에 Inject 메서드가 없습니다.", this);
                return;
            }

            var behaviours = FindObjectsOfType<MonoBehaviour>(true);

            foreach (var behaviour in behaviours)
            {
                if (!behaviour)
                    continue;

                var behaviourType = behaviour.GetType();

                if (!injectableInterfaceType.IsAssignableFrom(behaviourType))
                    continue;

                injectMethod.Invoke(behaviour, new object[] { injection });
            }
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(Injector))]
        private class InjectionManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Inject All"))
                    ((Injector)target).Inject();
            }
        }
#endif
    }
}
