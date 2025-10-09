using System;
using UnityEngine;
using UnityEngine.LowLevel;

namespace UniEngine
{
    /// <summary>
    /// 매 틱마다 실행되는 이벤트를 구독할 수 있는 클래스입니다.
    /// <para>
    /// UniTask, UniRx 등 외부 이벤트 프레임워크를 사용할 때 이 클래스를 비활성화하십시오. 충돌의 위험이 있습니다.
    /// </para>
    /// </summary> 
    public static class Loco
    {
        private static event Action Updates;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            var root = PlayerLoop.GetCurrentPlayerLoop();

            if (root.subSystemList == null || root.subSystemList.Length == 0)
                root = PlayerLoop.GetDefaultPlayerLoop();

            var system = new PlayerLoopSystem
            {
                type = typeof(Loco),
#pragma warning disable CS0618
                updateDelegate = LocoUpdate
#pragma warning restore CS0618
            };

            bool ok =
                TryInsert(ref root, typeof(UnityEngine.PlayerLoop.Update), system) ||
                TryInsert(ref root, typeof(UnityEngine.PlayerLoop.EarlyUpdate), system) ||
                TryInsert(ref root, typeof(UnityEngine.PlayerLoop.PreUpdate), system) ||
                TryInsert(ref root, typeof(UnityEngine.PlayerLoop.PostLateUpdate), system);

            if (ok)
                PlayerLoop.SetPlayerLoop(root);
            else
                Debug.LogWarning("[Loco] PlayerLoop 삽입 실패: 지원 단계(Update/Early/Pre/PostLate)를 찾지 못했습니다.");


            static bool TryInsert(ref PlayerLoopSystem loop, Type targetType, PlayerLoopSystem system)
            {
                var subs = loop.subSystemList;
                if (subs == null || subs.Length == 0) return false;

                for (int i = 0; i < subs.Length; i++)
                {
                    if (subs[i].type == targetType)
                    {
                        var inner = subs[i].subSystemList;

                        if (inner != null)
                        {
                            for (int k = 0; k < inner.Length; k++)
                            {
                                if (inner[k].type == system.type)
                                    return true;
                            }
                        }

                        if (inner == null || inner.Length == 0)
                        {
                            subs[i].subSystemList = new[] { system };
                        }
                        else
                        {
                            var newList = new PlayerLoopSystem[inner.Length + 1];
                            Array.Copy(inner, newList, inner.Length);
                            newList[inner.Length] = system;
                            subs[i].subSystemList = newList;
                        }

                        loop.subSystemList = subs;
                        return true;
                    }

                    if (subs[i].subSystemList != null && subs[i].subSystemList.Length > 0)
                    {
                        var child = subs[i];
                        if (TryInsert(ref child, targetType, system))
                        {
                            subs[i] = child;
                            loop.subSystemList = subs;
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        public static IDisposable Subscribe(Action action)
        {
            Updates += action;
            return new Handle(() => Updates -= action);
        }

        private static void LocoUpdate()
        {
            if (!Application.isPlaying)
            {
                Updates = null;
                return;
            }

            Updates?.Invoke();
        }
    }
}
