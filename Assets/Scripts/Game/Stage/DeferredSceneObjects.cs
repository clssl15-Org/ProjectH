using UnityEngine;

namespace Game.Stage
{
    /// <summary>
    /// 씬에 미리 배치된 오브젝트를 시작 시 비활성화하고, <see cref="Activate"/> 호출 시 활성화합니다.
    /// </summary>
    public class DeferredSceneObjects : MonoBehaviour
    {
        [SerializeField] private GameObject[] _objects;

        private void Awake()
        {
            SetActiveAll(false);
        }

        public void Activate()
        {
            SetActiveAll(true);
        }

        private void SetActiveAll(bool active)
        {
            if (_objects != null && _objects.Length > 0)
            {
                foreach (var obj in _objects)
                {
                    if (obj)
                        obj.SetActive(active);
                }

                return;
            }

            foreach (Transform child in transform)
                child.gameObject.SetActive(active);
        }
    }
}
