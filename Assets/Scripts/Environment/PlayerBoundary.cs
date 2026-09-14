using UnityEngine;

namespace Everplanet.Environment
{
    /// <summary>
    /// 플레이어가 맵(행성) 바깥으로 걸어나가지 못하도록 위치를 제한한다.
    /// CameraSystem.CameraFollow의 bounds와 같은 BoxCollider2D를 재사용하면 카메라 범위 = 플레이어
    /// 이동 가능 범위로 자연스럽게 맞아떨어진다.
    ///
    /// 필요한 세팅(Inspector):
    /// - Player 오브젝트에 이 스크립트 부착
    /// - bounds: 맵 전체를 감싸는 BoxCollider2D 연결 (Is Trigger 체크 - 물리 충돌에는 영향 없음)
    /// - skinWidth: 캐릭터 반지름 정도로 살짝 여유를 둬서 경계선에 반쯤 걸쳐 보이지 않게 함
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerBoundary : MonoBehaviour
    {
        public BoxCollider2D bounds;
        public float skinWidth = 0.3f;

        private Rigidbody2D rb;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void LateUpdate()
        {
            if (bounds == null)
            {
                return;
            }

            Bounds mapBounds = bounds.bounds;

            float minX = mapBounds.min.x + skinWidth;
            float maxX = mapBounds.max.x - skinWidth;
            float minY = mapBounds.min.y + skinWidth;
            float maxY = mapBounds.max.y - skinWidth;

            Vector2 position = rb.position;
            float clampedX = minX <= maxX ? Mathf.Clamp(position.x, minX, maxX) : mapBounds.center.x;
            float clampedY = minY <= maxY ? Mathf.Clamp(position.y, minY, maxY) : mapBounds.center.y;

            Vector2 clamped = new Vector2(clampedX, clampedY);
            if (clamped != position)
            {
                rb.position = clamped;
            }
        }
    }
}
