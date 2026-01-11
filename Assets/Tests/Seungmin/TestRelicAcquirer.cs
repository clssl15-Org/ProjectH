using UnityEngine;

public class TestRelicAcquirer : MonoBehaviour
{
    [field: SerializeField]
    public bool DirectAcquire { get; set; } = false;

    private void Update()
    {
        if (DirectAcquire && Input.GetKey(KeyCode.Tab))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                Add(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                Add(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                Add(3);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                Add(4);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                Add(5);

            void Add(int id)
            {
                RelicManager.Instance.AddRelic(id);
            }
        }
        else if (Input.GetKeyDown(KeyCode.Tab))
        {
            RelicManager.Instance.GetRandomRelicData();
        }
        else if (Input.GetKey(KeyCode.RightShift))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                Remove(1);
            else if (Input.GetKeyDown(KeyCode.Alpha2))
                Remove(2);
            else if (Input.GetKeyDown(KeyCode.Alpha3))
                Remove(3);
            else if (Input.GetKeyDown(KeyCode.Alpha4))
                Remove(4);
            else if (Input.GetKeyDown(KeyCode.Alpha5))
                Remove(5);

            void Remove(int id)
            {
                // 렐릭 삭제 시 처리
            }
        }
    }
}
