using UnityEngine;

namespace Everplanet.Planet
{
    /// <summary>
    /// 씬 안의 스폰 지점 하나를 표시하는 마커. 포탈의 spawnPointId와 같은 id를 가진
    /// SpawnPoint가 있으면 SceneSpawnHandler가 플레이어를 이 위치로 옮겨준다.
    /// </summary>
    public class SpawnPoint : MonoBehaviour
    {
        public string id;
        public bool isDefault;

        private void OnDrawGizmos()
        {
            Gizmos.color = isDefault ? Color.green : Color.cyan;
            Gizmos.DrawWireSphere(transform.position, 0.4f);
        }
    }
}
