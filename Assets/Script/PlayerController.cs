using System;
using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class PlayerController : MonoBehaviour
{
    [Header("DiChuyen")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;
    private float horizontal;

    [Header("Jump")]
    public float jumpForce = 10f; 
    private int jumpCount = 0;        
    private int maxJumpCount = 2;     
    private bool jumpRequested;       

    [Header("Attack / HP")] 
    public float maxHP = 100f;
    public float currentHP;
    public HpBar hpBar;

    // Cờ đánh dấu khi Player đang chạy animation đi vào/ra cửa
    private bool isTransitioning = false;
    
    [Header("Trap Settings")]
    public float trapDamageInterval = 2f; // Khoảng thời gian giữa các lần nhận sát thương từ Trap (2 giây)
    private float trapTimer = 0f;          // Bộ đếm thời gian cho Trap
    

    void Start()
    {
        currentHP = maxHP;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (hpBar != null)
        {
            hpBar.updateBar((int)currentHP, (int)maxHP);
        }
        
        CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();
        if (vcam != null)
        {
            vcam.Target.TrackingTarget = transform;
        }

        // Bắt đầu màn chơi bằng animation DoorOut
        StartCoroutine(PlayDoorOutRoutine());
    }

    private IEnumerator PlayDoorOutRoutine()
    {
        isTransitioning = true;
        rb.linearVelocity = Vector2.zero;

        // Chạy animation Player_DoorOut
        anim.SetTrigger("DoorOut");

        // Chờ animation hoàn thành (chỉnh lại thời gian khớp với độ dài clip DoorOut của bạn)
        yield return new WaitForSeconds(0.8f);

        isTransitioning = false;
    }

    // Hàm chui vào cửa nhận vị trí tâm cửa truyền từ DoorController
    public IEnumerator PlayDoorInRoutine(Vector3 doorCenterPosition)
    {
        isTransitioning = true;

        // 1. TẮT VẬT LÝ
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic; // Ngắt trọng lực & lực tác động vật lý
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;    // Tắt va chạm

        // 2. DỊCH CHUYỂN PLAYER VỀ CHÍNH GIỮA CỬA
        // Giữ nguyên trục Z của Player
        Vector3 targetPos = new Vector3(doorCenterPosition.x, doorCenterPosition.y, transform.position.z);
        float moveDuration = 0.2f; // Thời gian di chuyển về tâm cửa
        float elapsedTime = 0f;
        Vector3 startPos = transform.position;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(startPos, targetPos, elapsedTime / moveDuration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.position = targetPos; // Đảm bảo đặt đúng tâm

        // 3. CHẠY ANIMATION DOORIN
        anim.SetTrigger("DoorIn");

        // Chờ animation đi vào cửa kết thúc (chỉnh lại thời gian khớp với clip DoorIn của bạn)
        yield return new WaitForSeconds(0.8f);

        // 4. ẨN PLAYER SAU KHI ĐÃ VÀO CỬA
        sr.enabled = false;
    }

    void Update()
    {
        // Nếu đang chuyển cảnh/ra cửa/vào cửa hoặc đã chết thì không nhận phím
        if (isTransitioning || currentHP <= 0) return;

        if (Input.GetButtonDown("Jump") && jumpCount < maxJumpCount)
        {
            jumpRequested = true;
        }
        
        if (Input.GetKeyDown(KeyCode.J))
        {
            Attack();
        }

        UpdateAnimation();
    }
    
    void FixedUpdate()
    {
        if (isTransitioning || currentHP <= 0) 
        {
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            }
            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");
        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        rb.linearVelocity = new Vector2(horizontal * moveSpeed, rb.linearVelocity.y);

        if (horizontal > 0)
        {
            sr.flipX = false;
        }
        else if (horizontal < 0)
        {
            sr.flipX = true;
        }
    }

    void HandleJump()
    {
        if (jumpRequested)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpCount++;            
            jumpRequested = false;  
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                jumpCount = 0; 
                break;
            }
        }
    }

    void UpdateAnimation()
    {
        if (isTransitioning) return;

        if (jumpCount > 0)
        {
            anim.SetInteger("status", 2); // Jump
        }
        else if (horizontal != 0)
        {
            anim.SetInteger("status", 1); // Run
        }
        else
        {
            anim.SetInteger("status", 0); // Idle
        }
    }

    void Attack()
    {
        anim.SetTrigger("Attack"); 
        
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.attackSFX);
        }
    }

    public void TakeDamage(float damage)
    {
        if (isTransitioning) return;

        currentHP -= damage;
        currentHP = Mathf.Max(currentHP, 0);
        if (hpBar != null) hpBar.updateBar((int)currentHP, (int)maxHP);

        anim.SetTrigger("Hit");

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float heal)
    {
        currentHP += heal;
        if (hpBar != null) hpBar.updateBar((int)currentHP, (int)maxHP);
        if (currentHP > maxHP)
        {
            currentHP = maxHP;
        }
    }

    void Die()
    {
        anim.SetBool("Dead", true); 
        this.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning) return;

        if (other.CompareTag("Dimond"))
        {
            GameManager.Instance.AddScore(1);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.collectItemSFX);
            }
            Destroy(other.gameObject);
        }

        if (other.CompareTag("Hp_Item"))
        {
            Heal(10f);
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(AudioManager.Instance.collectItemSFX);
            }
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isTransitioning) return;

        // Nếu vẫn đang đứng trong Trap và đã trôi qua đủ 2s từ lần trừ máu trước
        if (other.CompareTag("Trap"))
        {
            if (Time.time >= trapTimer + trapDamageInterval)
            {
                TakeDamage(10f);
                trapTimer = Time.time; // Cập nhật lại mốc thời gian trừ máu
            }
        }
    }
}