using UnityEngine;

namespace Everplanet.CameraSystem
{
    /// <summary>
    /// 플레이어를 부드럽게 따라가는 탑다운 2D 카메라. 선택적으로 맵 경계를 벗어나지 않도록
    /// 카메라 이동 범위를 제한할 수 있다 (bounds를 지정하지 않으면 무제한으로 따라감).
    ///
    /// 필요한 세팅(Inspector):
    /// - Main Camera에 이 스크립트 부착
    /// - target: Player의 Transform 연결
    /// - (선택) bounds: 맵 전체를 감싸는 BoxCollider2D를 아무 오브젝트에 부착해두고 연결
    ///   (Is Trigger 체크해두면 물리에 영향 없음, 순수하게 경계 데이터로만 사용)
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [Header("추적 대상")]
        public Transform target;
        [Tooltip("값이 작을수록 더 빠르게(딱 붙어서) 따라감")]
        public float smoothTime = 0.15f;

        [Header("맵 경계 (선택 사항)")]
        [Tooltip("이 콜라이더의 범위 밖으로 카메라가 나가지 않도록 제한한다. 비워두면 제한 없음")]
        public BoxCollider2D bounds;

        private Vector3 velocity = Vector3.zero;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, transform.position.z);
            Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);

            if (bounds != null && cam != null)
            {
                smoothedPosition = ClampToBounds(smoothedPosition);
            }

            transform.position = smoothedPosition;
        }

        private Vector3 ClampToBounds(Vector3 position)
        {
            float halfHeight = cam.orthographicSize;
            float halfWidth = halfHeight * cam.aspect;

            Bounds mapBounds = bounds.bounds;

            float minX = mapBounds.min.x + halfWidth;
            float maxX = mapBounds.max.x - halfWidth;
            float minY = mapBounds.min.y + halfHeight;
            float maxY = mapBounds.max.y - halfHeight;

            // 맵이 카메라 화면보다 작을 경우(경계 안에서 클램프 범위가 뒤집히는 경우) 중앙으로 고정
            float clampedX = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : mapBounds.center.x;
            float clampedY = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : mapBounds.center.y;

            return new Vector3(clampedX, clampedY, position.z);
        }
    }
}
