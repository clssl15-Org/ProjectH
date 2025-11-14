using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target; // 추적할 플레이어의 Transform
    public float smoothSpeed = 0f;  // 카메라 얼마나 부드럽게 천천히 오게 만들건지. (0은 그냥 바로 따라옴)
    public Vector3 offset; // 플레이어로부터의 카메라 오프셋

    public float cameraMinX = -20; // 카메라의 최소 X 좌표
    public float cameraMaxX = 20; // 카메라의 최대 X 좌표
    public float cameraMinY = -10; // 카메라의 최소 Y 좌표
    public float cameraMaxY = 10; // 카메라의 최대 Y 좌표

    private Vector3 velocity = Vector3.zero;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = GetComponent<Camera>();
        if (mainCamera == null)
        {
            Debug.LogError("이 스크립트는 Camera 컴포넌트가 있는 오브젝트에 연결해야 합니다.");
            enabled = false;
        }
    }

    void LateUpdate()
    {
        if (target != null && mainCamera != null)
        {
            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

            // 카메라의 월드 좌표를 기준으로 제한
            float cameraHalfHeight = mainCamera.orthographicSize;
            float cameraHalfWidth = cameraHalfHeight * mainCamera.aspect;

            float clampedX = Mathf.Clamp(smoothedPosition.x, cameraMinX + cameraHalfWidth, cameraMaxX - cameraHalfWidth);
            float clampedY = Mathf.Clamp(smoothedPosition.y, cameraMinY + cameraHalfHeight, cameraMaxY - cameraHalfHeight);

            transform.position = new Vector3(clampedX, clampedY, transform.position.z);
        }
    }

    // 맵 경계를 시각적으로 확인하기 위한 Gizmos (선택 사항)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 minBounds = new Vector3(cameraMinX, cameraMinY, 0);
        Vector3 maxBounds = new Vector3(cameraMaxX, cameraMaxY, 0);

        // 최소/최대 경계 표시
        Gizmos.DrawWireCube((minBounds + maxBounds) / 2f, maxBounds - minBounds);

        // 카메라가 닿을 수 있는 실제 경계 (orthographicSize 고려)
        if (Camera.main != null)
        {
            float cameraHalfHeight = Camera.main.orthographicSize;
            float cameraHalfWidth = cameraHalfHeight * Camera.main.aspect;
            Gizmos.color = Color.green;
            Vector3 innerMin = new Vector3(cameraMinX + cameraHalfWidth, cameraMinY + cameraHalfHeight, 0);
            Vector3 innerMax = new Vector3(cameraMaxX - cameraHalfWidth, cameraMaxY - cameraHalfHeight, 0);
            Gizmos.DrawWireCube((innerMin + innerMax) / 2f, innerMax - innerMin);
        }
    }
}