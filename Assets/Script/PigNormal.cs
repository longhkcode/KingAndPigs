using UnityEngine;

public class PigNormal : EnemyController
{
    public enum EnemyState { Patrol, Chase, Attack }

    [Header("State Settings")]
    public EnemyState currentState = EnemyState.Patrol;

    [Header("Patrol Settings")]
    public Transform[] patrolPoints;
    private int currentPatrolPoint = 0;
    public float patrolSpeed = 1.5f;

    [Header("Chase Settings")]
    public float chaseRange = 3f;
    public float losePlayerRange = 5f;
    public float chaseSpeed = 3f;

    [Header("Attack Settings")]
    public float attackRange = 1.2f;
    public float attackCoolDown = 1f;
    private float nextAttack = 0f;

    private SpriteRenderer sr;

    protected override void Start()
    {
        base.Start(); // Gọi lớp cha để khởi tạo HP và Animator
        sr = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        // Tự tìm lại Player nếu lúc Start() chưa spawn kịp
        if (EnsurePlayerReference() == null) return;

        UpdateState();

        switch (currentState)
        {
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }

        UpdateAnimation();
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPatrolPoint];
        if (targetPoint == null) return;

        // Di chuyển theo trục X
        transform.position = Vector2.MoveTowards(
            transform.position,
            new Vector2(targetPoint.position.x, transform.position.y),
            Time.deltaTime * patrolSpeed
        );

        FlipSprite(targetPoint.position.x - transform.position.x);

        // CHỈ KIỂM TRA KHOẢNG CÁCH THEO TRỤC X
        if (Mathf.Abs(transform.position.x - targetPoint.position.x) <= 0.1f)
        {
            currentPatrolPoint = (currentPatrolPoint + 1) % patrolPoints.Length;
        }
    }

    private void Chase()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            new Vector2(player.transform.position.x, transform.position.y),
            Time.deltaTime * chaseSpeed
        );

        FlipSprite(player.transform.position.x - transform.position.x);
    }

    private void Attack()
    {
        FlipSprite(player.transform.position.x - transform.position.x);

        if (Time.time >= nextAttack)
        {
            nextAttack = Time.time + attackCoolDown;

            if (anim != null)
            {
                anim.ResetTrigger("Attack");
                anim.SetTrigger("Attack");
            }

            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }

    private void UpdateState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (distanceToPlayer <= chaseRange) currentState = EnemyState.Chase;
                break;

            case EnemyState.Chase:
                if (distanceToPlayer <= attackRange) currentState = EnemyState.Attack;
                else if (distanceToPlayer >= losePlayerRange) currentState = EnemyState.Patrol;
                break;

            case EnemyState.Attack:
                if (distanceToPlayer > attackRange) currentState = EnemyState.Chase;
                break;
        }
    }

    private void UpdateAnimation()
    {
        if (anim == null) return;
        bool isRunning = (currentState == EnemyState.Patrol || currentState == EnemyState.Chase);
        anim.SetBool("Run", isRunning);
    }

    private void FlipSprite(float directionX)
    {
        if (sr != null && directionX != 0)
        {
            sr.flipX = directionX > 0;
        }
    }
    // Hàm này CHỈ được gọi cho quái do Boss triệu hồi
    public void ForceChase()
    {
        currentState = EnemyState.Chase;
        chaseRange = 999f;       // Tăng tầm phát hiện
        losePlayerRange = 999f;  // Không quay lại Patrol
    }
}