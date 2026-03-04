using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Tests.Seungmin
{
    public class TestRelicAcquirer : MonoBehaviour
    {
        [SerializeField] private bool _addRelicsOnStart = true;
        [SerializeField] private int[] _relicsToAddOnStart;
        [Space]
        [SerializeField] private AcquireType _relicAcquireType = AcquireType.Random;
        [SerializeField] private bool _forceSuccess = false;
        [SerializeField] private int _relicID = 1;

        public enum AcquireType
        {
            Direct,
            Designated,
            Random
        }


        private void Start()
        {
            if (_addRelicsOnStart)
            {
                foreach (var relicId in _relicsToAddOnStart)
                    Acquire(relicId, AcquireType.Direct);
            }
        }

        public void Acquire() => Acquire(_relicID, _relicAcquireType);
        public void Acquire(int relicId, AcquireType acquireType = AcquireType.Random)
        {
            switch (acquireType)
            {
                case AcquireType.Direct:
                    RelicManager.Instance.AddRelic(relicId, out _, _forceSuccess);
                    break;

                case AcquireType.Designated:
                    RelicManager.Instance.GetRelicData(relicId, _forceSuccess);
                    break;

                case AcquireType.Random:
                    RelicManager.Instance.GetRandomRelicData();
                    break;

                default:
                    throw new System.ArgumentOutOfRangeException(
                        nameof(acquireType),
                        acquireType,
                        $"알 수 없는 AcquireType '{acquireType}'이(가) 입력되었습니다.");
            }
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(TestRelicAcquirer))]
        private class TestRelicAcquirerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
                GUILayout.Space(8);

                if (Application.isPlaying)
                {
                    var target = (TestRelicAcquirer)base.target;

                    if (GUILayout.Button("Add Relic"))
                        target.Acquire();
                }
                else
                    GUILayout.Label("Enter play mode to add skill", EditorStyles.centeredGreyMiniLabel);
            }
        }
#endif
    }
}
