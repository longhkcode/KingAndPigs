using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] protected float moveSpeed;
    [SerializeField] protected float maxHP = 50;
    [SerializeField] protected float currentHP;
    [SerializeField] protected float damage;

    protected PlayerController player;
    protected Animator anim;

    protected virtual void Start()
    {
        anim = GetComponent<Animator>();
        currentHP = maxHP;
        EnsurePlayerReference();
    }

    // Hàm hỗ trợ tự tìm Player bất cứ khi nào player bị null
    protected PlayerController EnsurePlayerReference()
    {
        if (player == null)
        {
            player = FindFirstObjectByType<PlayerController>();
        }
        return player;
    }

    public virtual void TakeDamage(float damageAmount)
    {
        currentHP -= damageAmount;
        currentHP = Mathf.Max(currentHP, 0);

        if (anim != null)
        {
            anim.SetTrigger("Hit");
        }

        if (currentHP <= 0)
        {
            Die();
        }
    }

    public virtual void Die()
    {
        // Báo cho GameManager cũ (nếu ở các Map thường)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.EnemyKilled();
        }
        // Báo cho GameManagerMap25 (nếu ở Map 25)
        else if (GameManagerMap25.Instance != null)
        {
            GameManagerMap25.Instance.EnemyKilled();
        }

        Destroy(gameObject);
    }
}