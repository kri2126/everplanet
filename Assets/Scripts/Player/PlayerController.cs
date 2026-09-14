using UnityEngine;
using Everplanet.Core;
using Everplanet.Combat;

namespace Everplanet.Player
{
    /// <summary>
    /// 플레이어 캐릭터의 이동/공격을 담당하는 컴포넌트. 탑다운 2D 기준(Rigidbody2D)으로 작성했다.
    /// 3D로 바꾸고 싶다면 Rigidbody2D -> Rigidbody, Vector2 -> Vector3(y 대신 z 사용) 정도만 손보면 된다.
    ///
    /// 필요한 세팅(Inspector):
    /// - Rigidbody2D 컴포넌트 부착, Gravity Scale = 0, Freeze Rotation Z 체크
    /// - Collider2D 부착 (예: CircleCollider2D)
    /// - Tag를 "Player"로 지정
    /// - attackPoint: 캐릭터 앞쪽에 빈 자식 오브젝트를 만들어 연결 (공격 판정 중심점)
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IDamageable
    {
        [Header("이동")]
        public float moveSpeed = 4f;

        [Header("공격")]
        public Transform attackPoint;
        public float attackRadius = 0.8f;
        public float attackCooldown = 0.5f;
        public LayerMask enemyLayer;

        private Rigidbody2D rb;
        private Vector2 moveInput;
        private float lastAttackTime = -999f;

        private PlayerStats Stats => GameManager.Instance != null ? GameManager.Instance.playerStats : null;

        // ---- IDamageable 구현: 실제 데이터는 GameManager의 PlayerStats에 위임 ----
        public int CurrentHP => Stats != null ? Stats.CurrentHP : 0;
        public int MaxHP => Stats != null ? Stats.MaxHP : 0;
        public bool IsDead => Stats != null ? Stats.IsDead : true;

        public void TakeDamage(int attackerPower)
        {
            Stats?.TakeDamage(attackerPower);
        }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        private void OnEnable()
        {
            if (Stats != null)
            {
                Stats.OnDeath += HandleDeath;
            }
        }

        private void OnDisable()
        {
            if (Stats != null)
            {
                Stats.OnDeath -= HandleDeath;
            }
        }

        private void Update()
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput = moveInput.normalized;

            if (Input.GetButtonDown("Fire1") || Input.GetKeyDown(KeyCode.Space))
            {
                TryAttack();
            }
        }

        private void FixedUpdate()
        {
            rb.MovePosition(rb.position + moveInput * moveSpeed * Time.fixedDeltaTime);
        }

        private void TryAttack()
        {
            if (Time.time - lastAttackTime < attackCooldown || attackPoint == null || Stats == null)
            {
                return;
            }

            lastAttackTime = Time.time;

            Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, enemyLayer);
            foreach (Collider2D hit in hits)
            {
                IDamageable damageable = hit.GetComponent<IDamageable>();
                damageable?.TakeDamage(Stats.AttackPower);
            }
        }

        private void HandleDeath()
        {
            GameManager.Instance?.HandlePlayerDeath();
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint == null)
            {
                return;
            }
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }
    }
}
