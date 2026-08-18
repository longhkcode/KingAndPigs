using UnityEngine;

public class BoxController : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damageToPlayer = 10f;          // Sát thương gây ra cho Player
    [SerializeField] private float stopVelocityThreshold = 0.1f;  // Ngưỡng vận tốc dừng hẳn
    
    [Header("Box HP")]
    [SerializeField] private float maxHP = 20f;                   // Máu của thùng
    private float currentHP;
    
    private Rigidbody2D rb;
    private Animator anim;
    private bool hasDealtDamage = false; // Đã gây sát thương cho Player chưa
    private bool isStopped = false;      // Đã nằm im hoàn toàn dưới đất chưa

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
    }
    private void Update()
    {
        // Kiểm tra xem thùng đã dừng di chuyển hoàn toàn dưới đất chưa
        if (!isStopped && !hasDealtDamage)
        {
            if (rb.linearVelocity.magnitude < stopVelocityThreshold && Mathf.Abs(rb.angularVelocity) < stopVelocityThreshold)
            {
                isStopped = true; // Thùng đã nằm im -> Hết khả năng gây sát thương
            }
        }
    }
    
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Nếu đã dừng hẳn hoặc đã trừ máu Player 1 lần rồi thì không gây sát thương nữa
        if (isStopped || hasDealtDamage) return;

        // Nếu chạm vào Player khi vẫn đang bay/nảy
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damageToPlayer);
                hasDealtDamage = true; // Trừ 10 máu xong thì thôi
            }
        }
    }
    // Hàm nhận sát thương (ví dụ khi Player đánh vào thùng)
    public void TakeDamage(float damageAmount)
    {
        anim.SetTrigger("Hit");
        currentHP -= damageAmount;

        if (currentHP <= 0)
        {
            DestroyBox();
        }
    }

    private void DestroyBox()
    {
        // Tùy chọn: Có thể Instantiate hiệu ứng vỡ thùng (vụm gỗ/bụi) ở đây nếu có
        Destroy(gameObject);
    }
}
