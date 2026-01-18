using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure
{
    /// <summary>
    /// Injection은 Start() 메서드가 호출되기 이전에 호출됩니다.
    /// </summary>
    public class Injector : MonoBehaviour
    {
        [Serializable]
        private struct Injection
        {
            public MonoBehaviour Item;
            public Type IntendedType;
        }

        [SerializeField] private List<Injection> _injections;
        [SerializeField] private bool _injectOnAwake = false;
        private bool _isInjected = false;

        private void Awake()
        {
            if (_injectOnAwake)
            {
                Debug.Log(Ctx(
                    $"'{nameof(_injectOnAwake)}'이(가) true이므로 Inject를 수행합니다. " +
                    $"GameManager를 사용중이라면 Inject는 GameManager에서만 이루어져야 합니다. " +
                    $"GameManager를 사용하지 않는 것이 의도된 동작인지 확인하세요."), this);

                Inject();
            }
        }

        public void AddInjection(MonoBehaviour injection, Type intendedType = null)
        {
            if (!injection)
            {
                Debug.LogError(
                    Ctx($"{nameof(injection)}이(가) 유효하지 않습니다."),
                    this);
                return;
            }

            _injections ??= new();
            _injections.Add(new Injection
            {
                Item = injection,
                IntendedType = intendedType
            });
        }

        public void Inject()
        {
            if (_isInjected)
            {
                Debug.LogError(
                    Ctx("이미 Inject가 수행되었습니다. 중복 실행을 방지합니다."), this);
                return;
            }

            _isInjected = true;


            foreach (var injection in _injections)
            {
                if (!injection.Item)
                {
                    Debug.LogError(
                        Ctx($"{nameof(_injections)} 배열에 유효하지 않은 항목이 있습니다."),
                        this);
                    continue;
                }

                Inject(injection);
            }
        }

        private void Inject(Injection injection)
        {
            if (!injection.Item)
            {
                Debug.LogError(
                    Ctx($"{nameof(injection)}이(가) 유효하지 않습니다."),
                    this);
                return;
            }

            var targetType = injection.IntendedType ?? injection.Item.GetType();
            var injectableInterfaceType = typeof(IInjectable<>).MakeGenericType(targetType);

            var injectMethod = injectableInterfaceType.GetMethod("Inject");
            if (injectMethod == null)
            {
                Debug.LogError(
                    Ctx($"IInjectable<{targetType.Name}> 에 Inject 메서드가 없습니다."),
                    this);
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

                injectMethod.Invoke(behaviour, new object[] { injection.Item });
            }
        }

        private string Ctx(string message) => $"[{nameof(Injector)}] {message}";

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
