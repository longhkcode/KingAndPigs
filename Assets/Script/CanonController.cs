using System.Collections;
using UnityEngine;

public class CanonController : MonoBehaviour
{
    [Header("Canon Settings")]
    public GameObject bombPrefab;
    public Transform firingPos;     // Vị trí bắn
    public float shootForce = 18f;  // Lực bắn
    
    private Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    public void Fire(Vector3 targetPosition)
    {
        if (bombPrefab == null) return;

        // 1. Kích hoạt Animation giật/nổ nòng pháo (Trigger "Fire" sẽ chuyển từ Idle -> Attack)
        if (anim != null)
        {
            anim.SetTrigger("Fire");
        }

        // 2. Bắn bom
        SpawnAndShootBomb(targetPosition);
    }

    private void SpawnAndShootBomb(Vector3 targetPosition)
    {
        Vector3 spawnPos = firingPos != null ? firingPos.position : transform.position;

        // Tạo quả bom
        GameObject bombInstance = Instantiate(bombPrefab, spawnPos, Quaternion.identity);

        // Bỏ qua va chạm giữa pháo, lợn và bom
        Collider2D canonCollider = GetComponent<Collider2D>();
        Collider2D pigCollider = GetComponentInParent<Collider2D>();
        Collider2D bombCollider = bombInstance.GetComponent<Collider2D>();

        if (bombCollider != null)
        {
            if (canonCollider != null) Physics2D.IgnoreCollision(canonCollider, bombCollider);
            if (pigCollider != null) Physics2D.IgnoreCollision(pigCollider, bombCollider);
        }

        // Tác động lực bắn tốc độ cao hướng tới Player
        Rigidbody2D bombRb = bombInstance.GetComponent<Rigidbody2D>();
        if (bombRb != null)
        {
            Vector2 targetDirection = (targetPosition - spawnPos).normalized;
            bombRb.AddForce(targetDirection * shootForce, ForceMode2D.Impulse);
        }

        // Kích hoạt bom ở chế độ Va chạm là Nổ Ngay
        BoomController boomController = bombInstance.GetComponent<BoomController>();
        if (boomController != null)
        {
            boomController.ActivateCanonBoom();
        }
    }
}