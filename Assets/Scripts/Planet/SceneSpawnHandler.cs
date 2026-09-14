using UnityEngine;
using Everplanet.Core;

namespace Everplanet.Planet
{
    /// <summary>
    /// 씬이 시작될 때 GameManager.PendingSpawnPointId를 확인해서 플레이어를 알맞은
    /// SpawnPoint 위치로 옮겨준다. 일치하는 id가 없으면 isDefault로 표시된 SpawnPoint를 사용한다.
    ///
    /// 필요한 세팅(Inspector):
    /// - 각 행성 씬에 빈 GameObject를 만들어 이 스크립트를 붙여둔다 (씬당 1개)
    /// - 씬 안에 SpawnPoint 컴포넌트를 가진 오브젝트를 하나 이상 배치 (그 중 하나는 isDefault = true)
    /// </summary>
    public class SceneSpawnHandler : MonoBehaviour
    {
        private void Start()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                return;
            }

            SpawnPoint[] spawnPoints = FindObjectsOfType<SpawnPoint>();
            if (spawnPoints.Length == 0)
            {
                return;
            }

            string targetId = GameManager.Instance != null ? GameManager.Instance.PendingSpawnPointId : null;
            SpawnPoint target = null;

            if (!string.IsNullOrEmpty(targetId))
            {
                foreach (SpawnPoint sp in spawnPoints)
                {
                    if (sp.id == targetId)
                    {
                        target = sp;
                        break;
                    }
                }
            }

            if (target == null)
            {
                foreach (SpawnPoint sp in spawnPoints)
                {
                    if (sp.isDefault)
                    {
                        target = sp;
                        break;
                    }
                }
            }

            if (target == null)
            {
                target = spawnPoints[0];
            }

            player.transform.position = target.transform.position;
        }
    }
}
