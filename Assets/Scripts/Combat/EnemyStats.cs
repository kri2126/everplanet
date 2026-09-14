using System;
using UnityEngine;
using Everplanet.Core;
using Everplanet.Data;

namespace Everplanet.Combat
{
    /// <summary>
    /// 몬스터의 체력/방어력 등 전투 데이터를 담당. EnemyDefinition(ScriptableObject)에서
    /// 기본 스탯을 읽어와 인스턴스별 현재 체력을 관리한다.
    ///
    /// 필요한 세팅(Inspector):
    /// - definition 필드에 EnemyDefinition 에셋 연결
    /// - Collider2D 부착, Layer를 "Enemy"로 지정 (PlayerController의 enemyLayer와 매칭)
    /// - Tag를 "Enemy"로 지정 (EnemyAI의 플레이어 탐지와 대칭되는 용도로 사용 가능)
    /// </summary>
    public class EnemyStats : MonoBehaviour, IDamageable
    {
        public EnemyDefinition definition;

        public int CurrentHP { get; private set; }
        public int MaxHP => definition != null ? definition.maxHP : 1;
        public bool IsDead { get; private set; }

        /// <summary>사망 시 발생. EnemyAI 등에서 구독해 상태를 Dead로 전환하는 데 사용.</summary>
        public event Action OnDeath;

        private void Awake()
        {
            CurrentHP = MaxHP;
        }

        public void TakeDamage(int attackerPower)
        {
            if (IsDead || definition == null)
            {
                return;
            }

            int damage = CombatUtility.CalculateDamage(attackerPower, definition.defensePower);
            CurrentHP = Mathf.Max(0, CurrentHP - damage);

            if (CurrentHP <= 0)
            {
                Die();
            }
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;

            if (GameManager.Instance != null && GameManager.Instance.playerStats != null && definition != null)
            {
                GameManager.Instance.playerStats.AddXP(definition.xpReward);
            }

            OnDeath?.Invoke();

            // 프로토타입은 즉시 제거. 나중에 사망 애니메이션/이펙트를 넣고 싶으면
            // 여기서 Destroy를 지연시키고 애니메이션 이벤트에서 Destroy를 호출하면 된다.
            Destroy(gameObject);
        }
    }
}
