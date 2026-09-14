using UnityEngine;
using Everplanet.Player;

namespace Everplanet.Core
{
    /// <summary>
    /// 프로토타입 범위의 간단한 저장/불러오기. PlayerPrefs를 사용한다.
    /// 나중에 파일(JSON) 저장으로 바꾸고 싶으면 이 클래스의 메서드 시그니처만 유지한 채
    /// 내부 구현만 교체하면 된다.
    /// </summary>
    public static class SaveSystem
    {
        private const string KeyLevel = "ep_save_level";
        private const string KeyXP = "ep_save_xp";
        private const string KeyPlanet = "ep_save_planet_scene";
        private const string KeyHasSave = "ep_save_exists";

        public static void Save(PlayerStats stats, string currentPlanetSceneName)
        {
            PlayerPrefs.SetInt(KeyLevel, stats.Level);
            PlayerPrefs.SetInt(KeyXP, stats.CurrentXP);
            PlayerPrefs.SetString(KeyPlanet, currentPlanetSceneName ?? string.Empty);
            PlayerPrefs.SetInt(KeyHasSave, 1);
            PlayerPrefs.Save();
        }

        public static bool HasSave()
        {
            return PlayerPrefs.GetInt(KeyHasSave, 0) == 1;
        }

        /// <summary>
        /// 저장된 값을 읽어 stats에 적용한다. 저장된 씬 이름을 반환한다(없으면 빈 문자열).
        /// </summary>
        public static string Load(PlayerStats stats)
        {
            if (!HasSave())
            {
                return string.Empty;
            }

            int savedLevel = PlayerPrefs.GetInt(KeyLevel, 1);
            int savedXP = PlayerPrefs.GetInt(KeyXP, 0);
            stats.LoadFromSave(savedLevel, savedXP);

            return PlayerPrefs.GetString(KeyPlanet, string.Empty);
        }

        public static void ClearSave()
        {
            PlayerPrefs.DeleteKey(KeyLevel);
            PlayerPrefs.DeleteKey(KeyXP);
            PlayerPrefs.DeleteKey(KeyPlanet);
            PlayerPrefs.DeleteKey(KeyHasSave);
        }
    }
}
