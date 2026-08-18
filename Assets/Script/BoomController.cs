using System.Collections;
using UnityEngine;

public class BoomController : MonoBehaviour
{
    [SerializeField] private GameObject vungNoObject; // Child VungNo
    private Animator anim;

    private bool isCanonBoom = false; // Phân biệt bom bắn từ pháo hay ném từ Pig
    private bool hasExploded = false;  // Tránh nổ 2 lần

    private void Awake()
    {
        anim = GetComponent<Animator>();
        if (vungNoObject != null)
        {
            vungNoObject.SetActive(false); // Ban đầu tắt vùng nổ
        }
    }

    #region 1. Dùng cho PigBomb (Đếm lùi 2s rồi nổ)
    public void ActivateBoom()
    {
        StartCoroutine(BoomRoutine());
    }

    private IEnumerator BoomRoutine()
    {
        anim.SetTrigger("On");
        yield return new WaitForSeconds(2f);
        Explode();
    }
    #endregion

    #region 2. Dùng cho Canon (Chạm là nổ ngay)
    public void ActivateCanonBoom()
    {
        isCanonBoom = true;
        anim.SetTrigger("On");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Nếu là bom của Canon VÀ chạm vào bất kỳ vật gì -> Nổ ngay
        if (isCanonBoom && !hasExploded)
        {
            Explode();
        }
    }
    #endregion

    // Hàm xử lý quy trình nổ chung cho cả 2 loại bom
    private void Explode()
    {
        if (hasExploded) return;
        hasExploded = true;

        // Dừng lực di chuyển của bom để nó đứng yên đúng vị trí nổ
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Kinematic;
        }

        // Bật vùng nổ gây sát thương & chạy animation nổ
        if (vungNoObject != null)
        {
            vungNoObject.SetActive(true);
        }
        anim.SetTrigger("Boooom");

        // Vụ nổ tồn tại 0.5s rồi Destroy
        StartCoroutine(DestroyRoutine());
    }

    private IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}