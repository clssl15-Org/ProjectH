using System;
using Actors.Monsters.Bosses;
using Infrastructure;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Actors.Monsters
{
    public class FallingStoneManager : MonoBehaviour
    {
        public float TimeInterval
        {
            get => _timeinterval;
            set => _timeinterval = Mathf.Max(0.01f, value);
        }
        public int StoneCount
        {
            get => _stoneCount;
            set => _stoneCount = Mathf.Max(0, value);
        }

        public bool IsFalling { get; private set; } = false;

        [Header("Prefabs")]
        [SerializeField] private FallingStone[] _stonePrefabs;
        [SerializeField] private GameObject _indicatorPrefab;
        [Header("Settings")]
        [SerializeField] private Transform _lt;
        [SerializeField] private Transform _rb;
        [SerializeField, Min(0)] private int _stoneCount = 10;
        [SerializeField, Min(0.01f)] private float _timeinterval = 1f;
        [SerializeField, Min(0.01f)] private float _minDistanceInterval = 1f;
        [SerializeField, Min(0)] private float _fallingStartHeightDelta = 1f;
        [SerializeField, Min(0.01f)] private float _gravityScale = 3f;
        [Header("Bindings")]
        [SerializeField] private Configuration _configuration;
        [SerializeField] private PlatformManager _platformManager;

        private Action<bool> _callback;

        private int _counter;
        private float _timer;
        private float _beforeX;
        private bool _succeed;


        public void Initialize(Configuration configuration, PlatformManager platformManager)
        {
            _configuration = configuration;
            _platformManager = platformManager;
        }

        public void DoFall(Action<bool> callback = null)
        {
            if (IsFalling) throw new InvalidOperationException(
                $"[{nameof(FallingStoneManager)}] 이미 동작이 진행 중이기 때문에 새 동작을 실행할 수 없습니다.");

            _callback = callback;
            _counter = 0;
            _timer = 0;

            IsFalling = true;
            _succeed = false;
            _beforeX = float.MaxValue;

            gameObject.SetActive(true);
        }

        private void Update()
        {
            if (!IsFalling)
                return;

            _timer -= Time.deltaTime;
            if (_timer > 0) return;

            // 마지막 Fall 이후 한 단계만큼 기다린 후 종료
            if (_counter >= _stoneCount)
            {
                _succeed = true;
                Done();
                return;
            }

            _timer = _timeinterval;

            #region Fall
            float xPos;
            int count = 0;
            do
            {
                xPos = UnityEngine.Random.Range(
                    _lt.transform.position.x, _rb.transform.position.x);

                count++;
                if (count > 100)
                {
                    Debug.LogWarning(
                        $"[{nameof(FallingStoneManager)}] 다음 낙하 위치를 정하는 데 실패하였습니다.\n" +
                        $"{nameof(_minDistanceInterval)}({_minDistanceInterval})이(가) 적용되지 않습니다.", this);
                    break;
                }

            } while (Mathf.Abs(_beforeX - xPos) < _minDistanceInterval);

            _beforeX = xPos;

            var indicator = Instantiate(_indicatorPrefab);
            indicator.transform.position = new Vector2
            {
                x = xPos,
                y = _rb.transform.position.y
            };
            indicator.SetActive(true);

            new Timer(5, _ =>
            {
                if (indicator)
                    Destroy(indicator);
            });

            new Timer(0.2f, _ =>
            {
                if (!IsFalling)
                    return;

                var stone = Instantiate(_stonePrefabs.GetRandomItem().gameObject)
                    .GetComponent<FallingStone>()
                    .Initialize(_configuration, _platformManager, _gravityScale);

                stone.transform.position = new Vector2
                {
                    x = xPos,
                    y = _lt.transform.position.y + _fallingStartHeightDelta,
                };
                stone.gameObject.SetActive(true);
            });
            #endregion

            _counter++;
        }

        public void CancelAction()
        {
            if (!IsFalling)
                return;

            Done();
        }

        private void Done()
        {
            if (!IsFalling)
                return;

            IsFalling = false;
            gameObject.SetActive(false);

            var callback = _callback;
            _callback = null;
            callback?.Invoke(_succeed);
        }


#if UNITY_EDITOR
        [CustomEditor(typeof(FallingStoneManager))]
        private class FallingStoneManagerEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();

                if (Application.isPlaying)
                {
                    var target = (FallingStoneManager)base.target;

                    if (!target.IsFalling)
                    {
                        if (GUILayout.Button("Test Fall"))
                            target.DoFall(null);
                    }
                    else
                    {
                        if (GUILayout.Button("Cancel"))
                            target.CancelAction();
                    }
                }
            }
        }
#endif
    }
}
