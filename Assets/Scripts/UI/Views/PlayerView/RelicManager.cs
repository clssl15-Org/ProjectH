using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UI.PlayerView
{
    [RequireComponent(typeof(RectTransform))]
    internal class RelicManager : MonoBehaviour
    {
        [SerializeField] private RelicUI _relicPrefab;
        [SerializeField] private SealUI _sealPrefab;
        [SerializeField, Min(0)] private int _maxRelicCount = 10;

        private RectTransform _transform;
        private readonly List<RelicUI> _relics = new();
        private SealUI _seal;


        private void Awake()
        {
            _transform = GetComponent<RectTransform>();

            foreach (Transform child in _transform)
                Destroy(child.gameObject);
        }

        public void AddRelic(RelicDataSO relicData)
        {
            var relicUI = Instantiate(_relicPrefab);

            relicUI.Initialize(relicData);
            relicUI.GetComponent<RectTransform>().SetParent(_transform);

            _relics.Add(relicUI);

            if (_relics.Count > _maxRelicCount)
            {
                if (_relics.Count == _maxRelicCount + 1)
                {
                    if (_seal)
                        Destroy(_seal);

                    _seal = Instantiate(_sealPrefab);
                    _seal.GetComponent<RectTransform>().SetParent(_transform);
                }

                _seal.SetCount(_relics.Count - _maxRelicCount);
                relicUI.gameObject.SetActive(false);
            }
            else
                relicUI.gameObject.SetActive(true);
        }

        public void RemoveRelic(int id)
        {
            var relicUI = _relics.LastOrDefault(rUI => rUI.ID == id);
            if (relicUI == default)
            {
                Debug.LogWarning(
                    $"[UI.{nameof(RelicManager)}] 입력 ID '{id}'와(과) 같은 식별자를 가지는 {nameof(RelicUI)}을(를) 찾지 못했습니다.",
                    this);
                return;
            }

            _relics.Remove(relicUI);

            if (relicUI && relicUI.gameObject)
                Destroy(relicUI.gameObject);

            if (_relics.Count <= _maxRelicCount)
            {
               if (_seal && _seal.gameObject)
                    Destroy(_seal.gameObject);
            }
            else
                _seal.SetCount(_relics.Count - _maxRelicCount);

            int idx = 0;
            foreach (var rUI in _relics)
            {  
                rUI.gameObject.SetActive(idx < _maxRelicCount);
                idx++;
            }
        }
    }
}
