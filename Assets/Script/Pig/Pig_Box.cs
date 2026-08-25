using UnityEngine;

public class PigBoxController : EnemyController
{
    [Header("Throwing Settings")]
    [SerializeField] private GameObject boxPrefab;        // Prefab chiếc thùng
    [SerializeField] private Transform throwPoint;        // Vị trí ném (trên tay/miệng con pig)
    [SerializeField] private float attackDistance = 6f;   // Tầm phát hiện Player
    [SerializeField] private float throwCooldown = 2f;    // Thời gian giữa 2 lần ném
    [SerializeField] private float throwForce = 10f;      // Lực ném thẳng tới Player

    private float cooldownTimer = 0f;

    protected override void Start()
    {
        base.Start(); // Đảm bảo lấy Animator và tìm Player từ EnemyController
    }

    protected void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        // Tự tìm lại Player nếu chưa có
        if (EnsurePlayerReference() == null) return;

        // Tính khoảng cách
        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        // Player vào tầm đánh VÀ hết cooldown
        if (distanceToPlayer <= attackDistance && cooldownTimer <= 0f)
        {
            ThrowBoxTowardsPlayer();
            cooldownTimer = throwCooldown;
        }
    }

    private void ThrowBoxTowardsPlayer()
    {
        if (boxPrefab == null) return;

        Vector3 spawnPos = throwPoint != null ? throwPoint.position : transform.position;

        // 1. Lật mặt Pig theo vị trí Player
        float direction = (player.transform.position.x > transform.position.x) ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);

        // Kích hoạt Animator Attack (nếu có)
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // 2. Tạo cái thùng
        GameObject boxInstance = Instantiate(boxPrefab, spawnPos, Quaternion.identity);

        // 3. Bỏ qua va chạm giữa Collider của Pig và Collider của Box
        Collider2D pigCollider = GetComponent<Collider2D>();
        Collider2D boxCollider = boxInstance.GetComponent<Collider2D>();

        if (pigCollider != null && boxCollider != null)
        {
            Physics2D.IgnoreCollision(pigCollider, boxCollider);
        }

        Rigidbody2D boxRb = boxInstance.GetComponent<Rigidbody2D>();

        if (boxRb != null)
        {
            Vector2 targetDirection = (player.transform.position - spawnPos).normalized;
            boxRb.AddForce(targetDirection * throwForce, ForceMode2D.Impulse);
            boxRb.AddTorque(-direction * 5f, ForceMode2D.Impulse);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}