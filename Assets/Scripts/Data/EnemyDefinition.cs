using UnityEngine;

namespace Everplanet.Data
{
    /// <summary>
    /// 몬스터 한 종류를 정의하는 데이터 에셋.
    /// Project 창에서 우클릭 -> Create -> Everplanet -> Enemy Definition 으로 생성.
    /// </summary>
    [CreateAssetMenu(fileName = "NewEnemy", menuName = "Everplanet/Enemy Definition")]
    public class EnemyDefinition : ScriptableObject
    {
        [Header("기본 정보")]
        public string enemyName = "New Enemy";

        [Header("스탯")]
        public int maxHP = 30;
        public int attackPower = 5;
        public int defensePower = 0;
        public int xpReward = 10;

        [Header("행동 파라미터")]
        public float moveSpeed = 2f;
        public float detectRange = 5f;
        public float attackRange = 1.2f;
        public float attackCooldown = 1.5f;
    }
}
