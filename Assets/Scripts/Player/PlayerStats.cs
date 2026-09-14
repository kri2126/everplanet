using System;
using UnityEngine;
using Everplanet.Combat;

namespace Everplanet.Player
{
    /// <summary>
    /// 플레이어의 성장 데이터(레벨/경험치/HP/공격력/방어력)를 담는 순수 데이터 + 로직 클래스.
    /// MonoBehaviour가 아니라 GameManager가 들고 있는 일반 클래스이므로, 씬이 바뀌어도
    /// GameManager(DontDestroyOnLoad)와 함께 값이 유지된다.
    /// </summary>
    [Serializable]
    public class PlayerStats
    {
        [Header("기본 성장 파라미터")]
        [SerializeField] private int level = 1;
        [SerializeField] private int currentXP = 0;

        [Header("레벨 1 기준 스탯")]
        [SerializeField] private int baseMaxHP = 50;
        [SerializeField] private int baseAttackPower = 8;
        [SerializeField] private int baseDefensePower = 2;

        [Header("레벨업당 증가량")]
        [SerializeField] private int hpPerLevel = 8;
        [SerializeField] private int atkPerLevel = 2;
        [SerializeField] private int defPerLevel = 1;

        [Header("경험치 곡선")]
        [Tooltip("다음 레벨 필요 경험치 = xpCurveBase * (레벨 ^ 1.5)")]
        [SerializeField] private float xpCurveBase = 20f;

        public int Level => level;
        public int CurrentXP => currentXP;
        public int MaxHP { get; private set; }
        public int CurrentHP { get; private set; }
        public int AttackPower { get; private set; }
        public int DefensePower { get; private set; }
        public bool IsDead => CurrentHP <= 0;

        public event Action<int> OnLevelUp;          // 새 레벨
        public event Action OnStatsChanged;           // HP/XP 등 값이 바뀔 때마다
        public event Action OnDeath;

        public int XPToNextLevel => Mathf.CeilToInt(xpCurveBase * Mathf.Pow(level, 1.5f));

        /// <summary>게임 시작 시 1회 호출해서 파생 스탯을 계산한다.</summary>
        public void Initialize()
        {
            RecalculateDerivedStats();
            CurrentHP = MaxHP;
            OnStatsChanged?.Invoke();
        }

        private void RecalculateDerivedStats()
        {
            int levelIndex = Mathf.Max(0, level - 1);
            MaxHP = baseMaxHP + hpPerLevel * levelIndex;
            AttackPower = baseAttackPower + atkPerLevel * levelIndex;
            DefensePower = baseDefensePower + defPerLevel * levelIndex;
        }

        public void AddXP(int amount)
        {
            if (amount <= 0 || IsDead)
            {
                return;
            }

            currentXP += amount;

            while (currentXP >= XPToNextLevel)
            {
                currentXP -= XPToNextLevel;
                level++;
                RecalculateDerivedStats();
                CurrentHP = MaxHP; // 레벨업 시 체력 전부 회복
                OnLevelUp?.Invoke(level);
            }

            OnStatsChanged?.Invoke();
        }

        public void TakeDamage(int attackerPower)
        {
            if (IsDead)
            {
                return;
            }

            int damage = CombatUtility.CalculateDamage(attackerPower, DefensePower);
            CurrentHP = Mathf.Max(0, CurrentHP - damage);
            OnStatsChanged?.Invoke();

            if (CurrentHP <= 0)
            {
                OnDeath?.Invoke();
            }
        }

        public void RestoreFullHP()
        {
            CurrentHP = MaxHP;
            OnStatsChanged?.Invoke();
        }

        /// <summary>SaveSystem에서 불러온 값을 적용할 때 사용.</summary>
        public void LoadFromSave(int savedLevel, int savedXP)
        {
            level = Mathf.Max(1, savedLevel);
            currentXP = Mathf.Max(0, savedXP);
            RecalculateDerivedStats();
            CurrentHP = MaxHP;
            OnStatsChanged?.Invoke();
        }
    }
}
