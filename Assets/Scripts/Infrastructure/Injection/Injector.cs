using System;
using System.Collections.Generic;
using System.Linq;
using BlackThunder.BlackboxSystem;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Infrastructure
{
    /// <summary>
    /// Injection�� Start() �޼��尡 ȣ��Ǳ� ������ ȣ��˴ϴ�.
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
        [Tooltip("GameManager�� Ȱ��ȭ�Ǿ� ���� ���� �� �� �׸��� üũ�Ͽ� Injector�� Ȱ��ȭ�� �� �ֽ��ϴ�.")]
        [SerializeField] private bool _injectOnAwake = false;
        private bool _isInjected = false;
        private BlackboxHandle _blackbox;

        private void Awake()
        {
            using var _ = BlackboxHandle.Of(this).Construct("Injector 초기화를 시작합니다.", out _blackbox);

            if (_injectOnAwake)
            {
                Debug.Log(Ctx(
                    $"'{nameof(_injectOnAwake)}'��(��) true�̹Ƿ� Inject�� �����մϴ�. " +
                    $"GameManager�� ������̶�� Inject�� GameManager������ �̷������ �մϴ�. " +
                    $"GameManager�� ������� �ʴ� ���� �ǵ��� �������� Ȯ���ϼ���."),
                    this);

                Inject();
            }
        }

        public bool HasInjection<T>() => _injections.Any(i => i.Item is T);

        public void AddInjection(MonoBehaviour injection, Type intendedType = null)
        {
            using var _ = _blackbox.Scope(
                intendedType != null
                    ? $"주입 객체를 등록합니다. intendedType: {intendedType.Name}"
                    : "주입 객체를 등록합니다.").With(injection);

            if (!injection)
            {
                Debug.LogError(Ctx(
                    $"{nameof(injection)}��(��) ��ȿ���� �ʽ��ϴ�."),
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
            using var _ = _blackbox.Scope("등록된 객체 주입을 시작합니다.");

            if (_isInjected)
            {
                Debug.LogWarning(Ctx("�̹� Inject�� ����Ǿ����ϴ�. �ߺ� ������ �����մϴ�."),
                    this);
                return;
            }

            _isInjected = true;


            foreach (var injection in _injections)
            {
                if (!injection.Item)
                {
                    Debug.LogError(Ctx($"{nameof(_injections)} �迭�� ��ȿ���� ���� �׸��� �ֽ��ϴ�."),
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
                Debug.LogError(Ctx($"{nameof(injection)}��(��) ��ȿ���� �ʽ��ϴ�."),
                    this);
                return;
            }

            var targetType = injection.IntendedType ?? injection.Item.GetType();
            var injectableInterfaceType = typeof(IInjectable<>).MakeGenericType(targetType);

            var injectMethod = injectableInterfaceType.GetMethod("Inject");
            if (injectMethod == null)
            {
                throw new InvalidOperationException(Ctx($"IInjectable<{targetType.Name}> �� Inject �޼��尡 �����ϴ�."));
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
