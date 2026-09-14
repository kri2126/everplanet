using UnityEngine;
using Everplanet.Data;

namespace Everplanet.Combat
{
    /// <summary>
    /// 아주 단순한 몬스터 AI 상태 머신: Idle -> Chase -> Attack.
    /// 확장하고 싶다면 Patrol 상태를 추가해 Idle 대신 정해진 경로를 순찰하게 만들면 된다.
    ///
    /// 필요한 세팅(Inspector):
    /// - Rigidbody2D 부착 (Gravity Scale 0, Freeze Rotation Z)
    /// - 같은 오브젝트의 EnemyStats.definition에 EnemyDefinition 에셋을 연결해두면
    ///   이 스크립트는 자동으로 같은 데이터를 가져다 쓴다 (따로 또 연결할 필요 없음)
    /// - 플레이어 오브젝트의 Tag가 "Player"로 지정되어 있어야 탐지 가능
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(EnemyStats))]
    public class EnemyAI : MonoBehaviour
    {
        private enum State { Idle, Chase, Attack, Dead }

        private EnemyDefinition definition;
        private Rigidbody2D rb;
        private EnemyStats stats;
        private Transform playerTransform;
        private State currentState = State.Idle;
        private float lastAttackTime = -999f;
        private Vector3 spawnPosition;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            stats = GetComponent<EnemyStats>();
            definition = stats.definition;
            spawnPosition = transform.position;
        }

        private void OnEnable()
        {
            stats.OnDeath += HandleDeath;
        }

        private void OnDisable()
        {
            stats.OnDeath -= HandleDeath;
        }

        private void Update()
        {
            if (currentState == State.Dead || definition == null)
            {
                return;
            }

            FindPlayerIfNeeded();

            if (playerTransform == null)
            {
                currentState = State.Idle;
                return;
            }

            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

            if (distanceToPlayer <= definition.attackRange)
            {
                currentState = State.Attack;
            }
            else if (distanceToPlayer <= definition.detectRange)
            {
                currentState = State.Chase;
            }
            else
            {
                currentState = State.Idle;
            }

            switch (currentState)
            {
                case State.Chase:
                    ChasePlayer();
                    break;
                case State.Attack:
                    AttackPlayer();
                    break;
                case State.Idle:
                default:
                    // 프로토타입에서는 제자리 대기. 필요하면 spawnPosition 주변 순찰 로직 추가.
                    break;
            }
        }

        private void FindPlayerIfNeeded()
        {
            if (playerTransform != null)
            {
                return;
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        private void ChasePlayer()
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            rb.MovePosition(rb.position + (Vector2)direction * definition.moveSpeed * Time.deltaTime);
        }

        private void AttackPlayer()
        {
            if (Time.time - lastAttackTime < definition.attackCooldown)
            {
                return;
            }

            lastAttackTime = Time.time;

            IDamageable playerDamageable = playerTransform.GetComponent<IDamageable>();
            playerDamageable?.TakeDamage(definition.attackPower);
        }

        private void HandleDeath()
        {
            currentState = State.Dead;
        }
    }
}
