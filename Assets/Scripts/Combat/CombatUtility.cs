using UnityEngine;

namespace Everplanet.Combat
{
    /// <summary>
    /// 데미지 계산 등 전투 관련 공용 수식을 모아두는 static 유틸리티.
    /// </summary>
    public static class CombatUtility
    {
        /// <summary>
        /// 기본 데미지 공식: 공격력 - 방어력, 최소 1.
        /// 나중에 크리티컬/속성 상성 등을 추가하고 싶으면 이 함수만 확장하면 된다.
        /// </summary>
        public static int CalculateDamage(int attackPower, int defensePower)
        {
            int raw = attackPower - defensePower;
            return Mathf.Max(1, raw);
        }
    }
}
