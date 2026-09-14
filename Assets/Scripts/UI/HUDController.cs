using UnityEngine;
using UnityEngine.UI;
using Everplanet.Core;
using Everplanet.Data;

namespace Everplanet.UI
{
    /// <summary>
    /// 체력바 / 레벨 / 경험치바 / 현재 행성 이름을 보여주는 기본 HUD.
    /// 씬마다 이 스크립트를 가진 UI Canvas를 하나씩 두면 된다 (GameManager와 달리 파괴되어도 됨).
    ///
    /// 필요한 세팅(Inspector):
    /// - hpSlider / xpSlider: UI - Slider (Min 0, Max 1, Whole Numbers 체크 해제)
    /// - levelText / planetNameText: UI - Text (또는 TextMeshPro로 바꿔도 무방, 그 경우 타입만 교체)
    /// </summary>
    public class HUDController : MonoBehaviour
    {
        public Slider hpSlider;
        public Slider xpSlider;
        public Text levelText;
        public Text planetNameText;

        private void OnEnable()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.playerStats.OnStatsChanged += RefreshStats;
            GameManager.Instance.playerStats.OnLevelUp += HandleLevelUp;
            GameManager.Instance.OnPlanetChanged += HandlePlanetChanged;

            RefreshStats();
            HandlePlanetChanged(GameManager.Instance.CurrentPlanet);
        }

        private void OnDisable()
        {
            if (GameManager.Instance == null)
            {
                return;
            }

            GameManager.Instance.playerStats.OnStatsChanged -= RefreshStats;
            GameManager.Instance.playerStats.OnLevelUp -= HandleLevelUp;
            GameManager.Instance.OnPlanetChanged -= HandlePlanetChanged;
        }

        private void RefreshStats()
        {
            var stats = GameManager.Instance.playerStats;

            if (hpSlider != null)
            {
                hpSlider.value = stats.MaxHP > 0 ? (float)stats.CurrentHP / stats.MaxHP : 0f;
            }

            if (xpSlider != null)
            {
                xpSlider.value = stats.XPToNextLevel > 0 ? (float)stats.CurrentXP / stats.XPToNextLevel : 0f;
            }

            if (levelText != null)
            {
                levelText.text = $"Lv. {stats.Level}";
            }
        }

        private void HandleLevelUp(int newLevel)
        {
            Debug.Log($"[HUD] 레벨업! 현재 레벨: {newLevel}");
            // 여기서 LevelUpPopup을 띄우도록 연결하면 된다.
        }

        private void HandlePlanetChanged(PlanetDefinition planet)
        {
            if (planetNameText != null)
            {
                planetNameText.text = planet != null ? planet.planetName : string.Empty;
            }
        }
    }
}
