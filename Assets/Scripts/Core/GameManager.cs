using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using Everplanet.Data;
using Everplanet.Player;

namespace Everplanet.Core
{
    /// <summary>
    /// 씬이 바뀌어도 파괴되지 않는 전역 싱글턴.
    /// 플레이어의 성장 데이터(PlayerStats)와 현재 행성 정보를 보관하고,
    /// 행성 이동(씬 전환) 요청의 진입점 역할을 한다.
    ///
    /// 최초 씬(마이 플래닛)에 빈 GameObject를 만들고 이 스크립트를 붙여두면 된다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("플레이어 성장 데이터")]
        public PlayerStats playerStats;

        [Header("행성 정보")]
        [Tooltip("게임 시작 시점의 행성 (보통 마이 플래닛)")]
        public PlanetDefinition startingPlanet;

        public PlanetDefinition CurrentPlanet { get; private set; }

        /// <summary>다음 씬 로드 후 플레이어가 스폰될 위치 이름표(옵션)</summary>
        public string PendingSpawnPointId { get; private set; }

        public event Action<PlanetDefinition> OnPlanetChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (playerStats == null)
            {
                playerStats = new PlayerStats();
            }
            playerStats.Initialize();

            CurrentPlanet = startingPlanet;

            // 저장된 데이터가 있으면 레벨/경험치를 덮어씌운다 (HP는 Initialize에서 이미 만땅으로 설정됨).
            if (SaveSystem.HasSave())
            {
                SaveSystem.Load(playerStats);
            }
        }

        /// <summary>현재 레벨/경험치/행성 정보를 저장한다. 메뉴 버튼이나 자동 저장 시점에서 호출.</summary>
        public void SaveGame()
        {
            string sceneName = CurrentPlanet != null ? CurrentPlanet.sceneName : SceneManager.GetActiveScene().name;
            SaveSystem.Save(playerStats, sceneName);
        }

        private void OnApplicationQuit()
        {
            SaveGame();
        }

        /// <summary>
        /// 행성으로 이동을 시도한다. 레벨 조건을 만족하지 못하면 false를 반환하고 이동하지 않는다.
        /// </summary>
        public bool TryTravelToPlanet(PlanetDefinition destination, string spawnPointId = null)
        {
            if (destination == null || string.IsNullOrEmpty(destination.sceneName))
            {
                Debug.LogWarning("[GameManager] 목적지 행성 정보가 비어있습니다.");
                return false;
            }

            if (playerStats.Level < destination.requiredLevel)
            {
                Debug.Log($"[GameManager] {destination.planetName} 입장 불가 - 요구 레벨 {destination.requiredLevel}, 현재 레벨 {playerStats.Level}");
                return false;
            }

            PendingSpawnPointId = spawnPointId;
            CurrentPlanet = destination;
            OnPlanetChanged?.Invoke(destination);
            SaveGame();
            SceneManager.LoadScene(destination.sceneName);
            return true;
        }

        /// <summary>
        /// 플레이어 사망 시 호출. 리스폰 대상 행성으로 돌려보낸다.
        /// </summary>
        public void HandlePlayerDeath()
        {
            playerStats.RestoreFullHP();

            PlanetDefinition respawnTarget = CurrentPlanet != null ? CurrentPlanet.respawnPlanet : null;
            if (respawnTarget != null)
            {
                CurrentPlanet = respawnTarget;
                OnPlanetChanged?.Invoke(respawnTarget);
                SceneManager.LoadScene(respawnTarget.sceneName);
            }
            else
            {
                // 리스폰 대상이 지정되어 있지 않으면 현재 씬을 그냥 리로드한다.
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            }
        }
    }
}
