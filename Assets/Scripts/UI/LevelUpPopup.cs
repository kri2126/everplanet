using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Everplanet.Core;

namespace Everplanet.UI
{
    /// <summary>
    /// 레벨업 시 잠깐 나타났다 사라지는 간단한 안내 팝업.
    ///
    /// 주의: 이 스크립트가 붙은 오브젝트 자체는 항상 활성화 상태여야 한다(OnEnable에서 이벤트를
    /// 구독하기 때문에, 오브젝트가 처음부터 비활성화되어 있으면 구독 자체가 되지 않는다).
    /// 대신 실제로 보이고 숨겨지는 것은 panel 자식 오브젝트다.
    ///
    /// 필요한 세팅(Inspector):
    /// - 이 스크립트는 항상 활성화된 오브젝트(예: HUD Canvas의 자식)에 부착
    /// - panel: 실제 배경/텍스트를 담은 자식 오브젝트 (평소 비활성화 상태로 시작)
    /// - messageText: panel 안의 UI - Text
    /// </summary>
    public class LevelUpPopup : MonoBehaviour
    {
        public GameObject panel;
        public Text messageText;
        public float visibleDuration = 1.5f;

        private Coroutine hideRoutine;

        private void Awake()
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerStats.OnLevelUp += Show;
            }
        }

        private void OnDisable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.playerStats.OnLevelUp -= Show;
            }
        }

        private void Show(int newLevel)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }

            if (messageText != null)
            {
                messageText.text = $"레벨 업! Lv.{newLevel}";
            }

            if (hideRoutine != null)
            {
                StopCoroutine(hideRoutine);
            }
            hideRoutine = StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(visibleDuration);
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
    }
}
