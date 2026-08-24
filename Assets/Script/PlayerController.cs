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

    // Biến kiểm tra game đã thua chưa
    public bool gameLose = false;

    // Cờ đánh dấu khi Player đang chạy animation đi vào/ra cửa
    private bool isTransitioning = false;

    [Header("Trap Settings")]
    public float trapDamageInterval = 2f; // Khoảng thời gian giữa các lần nhận sát thương từ Trap (2 giây)
    private float trapTimer = 0f;          // Bộ đếm thời gian cho Trap


    void Start()
    {
        // Đảm bảo game bắt đầu bình thường
        Time.timeScale = 1f;

        // Reset trạng thái Game Lose
        gameLose = false;

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

        // Chờ animation hoàn thành
        yield return new WaitForSeconds(0.8f);

        isTransitioning = false;
    }

    // Hàm chui vào cửa nhận vị trí tâm cửa truyền từ DoorController
    public IEnumerator PlayDoorInRoutine(Vector3 doorCenterPosition)
    {
        isTransitioning = true;

        // 1. TẮT VẬT LÝ
        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Collider2D col = GetComponent<Collider2D>();

        if (col != null)
        {
            col.enabled = false;
        }

        // 2. DỊCH CHUYỂN PLAYER VỀ CHÍNH GIỮA CỬA
        // Giữ nguyên trục Z của Player
        Vector3 targetPos = new Vector3(
            doorCenterPosition.x,
            doorCenterPosition.y,
            transform.position.z
        );

        float moveDuration = 0.2f;
        float elapsedTime = 0f;

        Vector3 startPos = transform.position;

        while (elapsedTime < moveDuration)
        {
            transform.position = Vector3.Lerp(
                startPos,
                targetPos,
                elapsedTime / moveDuration
            );

            elapsedTime += Time.deltaTime;

            yield return null;
        }

        transform.position = targetPos;

        // 3. CHẠY ANIMATION DOORIN
        anim.SetTrigger("DoorIn");

        // Chờ animation đi vào cửa kết thúc
        yield return new WaitForSeconds(0.8f);

        // 4. ẨN PLAYER SAU KHI ĐÃ VÀO CỬA
        sr.enabled = false;
    }

    void Update()
    {
        // Nếu đang chuyển cảnh hoặc đã chết thì không nhận phím
        if (isTransitioning || gameLose || currentHP <= 0)
            return;

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
        if (isTransitioning || gameLose || currentHP <= 0)
        {
            if (rb.bodyType == RigidbodyType2D.Dynamic)
            {
                rb.linearVelocity = new Vector2(
                    0,
                    rb.linearVelocity.y
                );
            }

            return;
        }

        horizontal = Input.GetAxisRaw("Horizontal");

        HandleMovement();
        HandleJump();
    }

    void HandleMovement()
    {
        rb.linearVelocity = new Vector2(
            horizontal * moveSpeed,
            rb.linearVelocity.y
        );

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
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

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
        if (isTransitioning || gameLose)
            return;

        if (jumpCount > 0)
        {
            anim.SetInteger("status", 2);
        }
        else if (horizontal != 0)
        {
            anim.SetInteger("status", 1);
        }
        else
        {
            anim.SetInteger("status", 0);
        }
    }

    void Attack()
    {
        anim.SetTrigger("Attack");

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(
                AudioManager.Instance.attackSFX
            );
        }
    }

    public void TakeDamage(float damage)
    {
        // Không nhận damage nếu đang chuyển cửa hoặc game đã thua
        if (isTransitioning || gameLose)
            return;

        currentHP -= damage;

        // Không cho HP xuống dưới 0
        currentHP = Mathf.Max(currentHP, 0);

        // Cập nhật thanh máu
        if (hpBar != null)
        {
            hpBar.updateBar(
                (int)currentHP,
                (int)maxHP
            );
        }

        // Nếu HP vẫn còn thì chạy animation Hit
        if (currentHP > 0)
        {
            anim.SetTrigger("Hit");
        }

        // Nếu HP <= 0 thì Game Lose
        if (currentHP <= 0)
        {
            Die();
        }
    }

    public void Heal(float heal)
    {
        // Không hồi máu nếu đã thua
        if (gameLose)
            return;

        currentHP += heal;

        // Không cho HP vượt quá Max HP
        currentHP = Mathf.Min(currentHP, maxHP);

        // Cập nhật thanh máu
        if (hpBar != null)
        {
            hpBar.updateBar(
                (int)currentHP,
                (int)maxHP
            );
        }
    }

    void Die()
    {
        if (gameLose) return;

        gameLose = true;
        currentHP = 0;

        if (hpBar != null)
        {
            hpBar.updateBar(0, (int)maxHP);
        }

        // Kích hoạt animation chết
        anim.SetBool("Dead", true);

        // Gọi GameManager bật Lose UI và dừng thời gian
        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver();
        }

        this.enabled = false;
        Debug.Log("GAME LOSE - Player đã hết máu!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTransitioning || gameLose)
            return;

        if (other.CompareTag("Dimond"))
        {
            GameManager.Instance.AddScore(1);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    AudioManager.Instance.collectItemSFX
                );
            }

            Destroy(other.gameObject);
        }

        if (other.CompareTag("Hp_Item"))
        {
            Heal(10f);

            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySFX(
                    AudioManager.Instance.collectItemSFX
                );
            }

            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isTransitioning || gameLose)
            return;

        // Nếu vẫn đang đứng trong Trap
        // và đã đủ 2 giây từ lần nhận damage trước
        if (other.CompareTag("Trap"))
        {
            if (Time.time >= trapTimer + trapDamageInterval)
            {
                TakeDamage(10f);

                trapTimer = Time.time;
            }
        }
    }
}