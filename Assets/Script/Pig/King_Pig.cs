using System.Collections;
using UnityEngine;

public class King_Pig : EnemyController
{
    public enum BossState
    {
        Chase,
        Attack
    }

    [Header("Boss State")]
    public BossState currentState = BossState.Chase;

    [Header("Skill Prefabs & Positions")]
    public GameObject bombPrefab;
    public GameObject boxPrefab;
    public GameObject basicEnemyPrefab;
    public Transform firePos;

    [Header("Distance & Movement Settings")]
    public float chaseRange = 10f;        // Khoảng cách bắt đầu nhận diện & đuổi Player
    public float attackRange = 1.2f;      // Khoảng cách vào tầm đánh cận chiến
    public float fireSpeed = 10f;         // Tốc độ bay của Bom & Thùng gỗ

    [Header("Melee Attack Settings")]
    public float attackCoolDown = 1f;      // Hồi chiêu đánh cận chiến
    private float nextAttack = 0f;

    [Header("Skill Cooldowns")]
    public float skillCooldown = 6f;      // Thời gian giữa các lần dùng skill
    private float nextTimeSkill = 0f;

    [Header("Telegraph / Warning Line")]
    public LineRenderer lineRendererPrefab; 
    public float warningDuration = 1.5f;   

    private SpriteRenderer sr;

    protected override void Start()
    {
        base.Start();
        sr = GetComponent<SpriteRenderer>();

        if (firePos == null)
        {
            firePos = transform;
        }
    }
    
    protected void Update()
    {
        // Kiểm tra máu và tự động tìm Player nếu chưa gán
        if (currentHP <= 0 || EnsurePlayerReference() == null) return;

        // 1. Cập nhật trạng thái Chase / Attack
        UpdateState();

        // 2. Thực thi hành vi theo State
        switch (currentState)
        {
            case BossState.Chase:
                Chase();
                break;
            case BossState.Attack:
                Attack();
                break;
        }

        // 3. Cập nhật Animation "Run"
        UpdateAnimation();

        // 4. Tung Skill ngẫu nhiên
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);
        if (Time.time >= nextTimeSkill && distanceToPlayer <= chaseRange)
        {
            UseRandomSkill();
            nextTimeSkill = Time.time + skillCooldown;
        }
    }
    
    private void UpdateState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        switch (currentState)
        {
            case BossState.Chase:
                if (distanceToPlayer <= attackRange)
                {
                    currentState = BossState.Attack;
                }
                break;

            case BossState.Attack:
                if (distanceToPlayer > attackRange)
                {
                    currentState = BossState.Chase;
                }
                break;
        }
    }
    
    private void FlipSprite(float directionX)
    {
        if (sr != null && directionX != 0)
        {
            sr.flipX = directionX > 0;
        }
        else if (directionX != 0)
        {
            float scaleX = Mathf.Abs(transform.localScale.x);
            transform.localScale = new Vector3(directionX < 0 ? scaleX : -scaleX, transform.localScale.y, transform.localScale.z);
        }
    }
    
    private void Chase()
    {
        transform.position = Vector2.MoveTowards(
            transform.position,
            player.transform.position,
            moveSpeed * Time.deltaTime
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
                anim.SetTrigger("Attack");
            }

            if (player != null)
            {
                player.TakeDamage(damage);
            }
        }
    }
    
    private void UpdateAnimation()
    {
        if (anim == null) return;
        bool isRunning = (currentState == BossState.Chase);
        anim.SetBool("Run", isRunning);
    }
    
    private void UseRandomSkill()
    {
        int randomSkill = Random.Range(1, 6);

        switch (randomSkill)
        {
            case 1:
                StartCoroutine(BanBombWithWarning());
                break;
            case 2:
                StartCoroutine(BanThungGoWithWarning());
                break;
            case 3:
                Health();
                break;
            case 4:
                CreatEnemy();
                break;
            case 5:
                Teleport();
                break;
        }
    }
    
// Skill 1: Bắn Bom
    private IEnumerator BanBombWithWarning()
    {
        LineRenderer warningLine = null;
        if (lineRendererPrefab != null)
        {
            warningLine = Instantiate(lineRendererPrefab, firePos.position, Quaternion.identity);
        }
    
        Vector3 targetDirection = Vector3.zero;
        float timer = 0f;

        while (timer < warningDuration)
        {
            timer += Time.deltaTime;

            if (player != null)
            {
                targetDirection = (player.transform.position - firePos.position).normalized;
            }

            if (warningLine != null)
            {
                warningLine.SetPosition(0, firePos.position);
                warningLine.SetPosition(1, firePos.position + targetDirection * 15f);
            }
            yield return null;
        }
    
        if (warningLine != null) Destroy(warningLine.gameObject);

        if (bombPrefab != null)
        {
            GameObject bomb = Instantiate(bombPrefab, firePos.position, Quaternion.identity);

            // Bỏ qua va chạm giữa Boss và Quả Bom
            Collider2D bossCollider = GetComponentInChildren<Collider2D>();
            Collider2D bombCollider = bomb.GetComponentInChildren<Collider2D>();
            if (bossCollider != null && bombCollider != null)
            {
                Physics2D.IgnoreCollision(bossCollider, bombCollider);
            }

            BoomController boomCtrl = bomb.GetComponent<BoomController>();
            if (boomCtrl != null)
            {
                boomCtrl.ActivateCanonBoom();
            }

            Rigidbody2D rb = bomb.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = targetDirection * (fireSpeed * 1.5f); // Bắn thẳng theo hướng Player
            }
            if (anim != null) anim.SetTrigger("Attack");
        }
    }
    
    // Skill 2: Bắn Thùng Gỗ Xung Quanh    
    private IEnumerator BanThungGoWithWarning()
    {
        int boxCount = 6;
        float angleStep = 360f / boxCount;
        
        LineRenderer[] warningLines = new LineRenderer[boxCount];
        if (lineRendererPrefab != null)
        {
            for (int i = 0; i < boxCount; i++)
            {
                warningLines[i] = Instantiate(lineRendererPrefab, firePos.position, Quaternion.identity);
            }
        }

        float timer = 0;
        while (timer < warningDuration)
        {
            timer += Time.deltaTime;
            if (lineRendererPrefab != null)
            {
                for (int i = 0; i < boxCount; i++)
                {
                    if (warningLines[i] != null)
                    {
                        float angle = i * angleStep;
                        Vector3 dir = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0);
                        
                        warningLines[i].SetPosition(0, transform.position);
                        warningLines[i].SetPosition(1, transform.position + dir * 10f);
                    }
                }
            }
            yield return null;
        }
        
        for (int i = 0; i < boxCount; i++)
        {
            if (warningLines[i] != null) Destroy(warningLines[i].gameObject);
        }

        if (boxPrefab != null)
        {
            Collider2D bossCollider = GetComponentInChildren<Collider2D>();

            for (int i = 0; i < boxCount; i++)
            {
                float angle = i * angleStep;
                Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad)).normalized;

                GameObject box = Instantiate(boxPrefab, firePos.position, Quaternion.identity);

                // Bỏ qua va chạm giữa Boss và Thùng Gỗ
                Collider2D boxCollider = box.GetComponentInChildren<Collider2D>();
                if (bossCollider != null && boxCollider != null)
                {
                    Physics2D.IgnoreCollision(bossCollider, boxCollider);
                }

                Rigidbody2D rb = box.GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.linearVelocity = dir * fireSpeed; // Gán vận tốc trực tiếp giúp thùng bay ra lập tức
                }
                Destroy(box, 4f);
            }
            if (anim != null) anim.SetTrigger("Attack");
        }
    }
    
    // Skill 3: Heal
    private void Health()
    {
        float healAmount = maxHP * 0.15f;
        currentHP = Mathf.Min(currentHP + healAmount, maxHP);

        if (anim != null) anim.SetTrigger("Heal");
    }
    
    // Skill 4: Tạo Enemy Xung quanh    
    private void CreatEnemy()
    {
        if (basicEnemyPrefab == null) return;

        Vector3 spawnOffset = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), 0f); 
        GameObject newEnemy = Instantiate(basicEnemyPrefab, transform.position + spawnOffset, Quaternion.identity);

        // Bỏ qua va chạm giữa Boss và Quái mới sinh
        Collider2D bossCollider = GetComponentInChildren<Collider2D>();
        Collider2D enemyCollider = newEnemy.GetComponentInChildren<Collider2D>();
        if (bossCollider != null && enemyCollider != null)
        {
            Physics2D.IgnoreCollision(bossCollider, enemyCollider);
        }

        // CHỈ ép con quái mới tạo này lao vào đuổi Player ngay lập tức
        PigNormal pigScript = newEnemy.GetComponent<PigNormal>();
        if (pigScript != null)
        {
            pigScript.ForceChase();
        }
    }

    // Skill 5: Dịch chuyển tức thời
    private void Teleport()
    {
        if (player == null) return;
        Vector3 offset = new Vector3(-1f, 0f, 0f); 
        transform.position = player.transform.position + offset;
    }
    public override void Die()
    {
        // Báo cho GameManagerMap25 biết Boss đã bị tiêu diệt
        if (GameManagerMap25.Instance != null)
        {
            GameManagerMap25.Instance.BossKilled();
        }

        Destroy(gameObject);
    }
    
}