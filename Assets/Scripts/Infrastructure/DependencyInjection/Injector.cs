using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure
{
    public class Injector : MonoBehaviour
    {
        [SerializeField] private MonoBehaviour _item;
        [SerializeField] private bool _injectAtAwake = false;

        private void Awake()
        {
            if (_injectAtAwake && _item)
                Inject();
        }

        public void Inject()
        {
            if (!_item)
            {
                Debug.LogWarning($"{nameof(Injector)} on '{name}': _item 이 설정되지 않았습니다.", this);
                return;
            }

            var targetType = _item.GetType();
            var injectableInterfaceType = typeof(IInjectable<>).MakeGenericType(targetType);

            var injectMethod = injectableInterfaceType.GetMethod("Inject");
            if (injectMethod == null)
            {
                Debug.LogError($"IInjectable<{targetType.Name}> 에 Inject 메서드가 없습니다.");
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

                injectMethod.Invoke(behaviour, new object[] { _item });
            }
        }

#if UNITY_EDITOR
        [CustomEditor(typeof(Injector))]
        private class InjectorEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (GUILayout.Button("Inject"))
                    ((Injector)target).Inject();
            }
        }
#endif
    }
}
