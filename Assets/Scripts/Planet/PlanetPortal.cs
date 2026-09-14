using UnityEngine;
using Everplanet.Core;
using Everplanet.Data;

namespace Everplanet.Planet
{
    /// <summary>
    /// 마이 플래닛(또는 다른 행성)에 배치하는 이동 포탈. 플레이어가 범위 안에 들어온 상태에서
    /// 상호작용 키(기본 E)를 누르면 목적지 행성으로 이동을 시도한다.
    /// 레벨이 부족하면 GameManager가 이동을 막고, 이 스크립트는 안내 문구를 보여준다.
    ///
    /// 필요한 세팅(Inspector):
    /// - Collider2D 부착 후 Is Trigger 체크
    /// - destination 필드에 이동할 PlanetDefinition 에셋 연결
    /// - (선택) promptText에 "E키를 눌러 이동" 같은 UI Text/TMP 오브젝트 연결 - 평소엔 비활성화해두면
    ///   플레이어가 범위에 들어왔을 때만 자동으로 표시된다.
    /// </summary>
    public class PlanetPortal : MonoBehaviour
    {
        public PlanetDefinition destination;
        public string spawnPointId;
        public KeyCode interactKey = KeyCode.E;

        [Tooltip("선택 사항: 범위 안에 들어왔을 때 활성화할 안내 UI 오브젝트")]
        public GameObject promptUI;

        private bool playerInRange;

        private void Update()
        {
            if (playerInRange && Input.GetKeyDown(interactKey))
            {
                bool success = GameManager.Instance != null && GameManager.Instance.TryTravelToPlanet(destination, spawnPointId);
                if (!success)
                {
                    Debug.Log($"[PlanetPortal] {destination?.planetName} 이동 실패 - 레벨이 부족하거나 목적지 정보가 없습니다.");
                }
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInRange = true;
            SetPromptVisible(true);
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
            {
                return;
            }

            playerInRange = false;
            SetPromptVisible(false);
        }

        private void SetPromptVisible(bool visible)
        {
            if (promptUI != null)
            {
                promptUI.SetActive(visible);
            }
        }
    }
}
