using UnityEngine;

public class PigBomb : EnemyController
{
    [Header("Settings")]
    public GameObject bombPrefab;
    public Transform throwPoint;
    public float attackDistance = 6f;
    public float timeBreak = 2f;

    [SerializeField] private float throwForce = 8f;
    [SerializeField] private float throwUpForce = 3f;

    private float cooldownTimer;

    protected override void Start()
    {
        base.Start(); // Bắt buộc gọi base.Start()
    }

    private void Update()
    {
        if (cooldownTimer > 0)
        {
            cooldownTimer -= Time.deltaTime;
        }

        if (EnsurePlayerReference() == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.transform.position);

        if (distanceToPlayer <= attackDistance && cooldownTimer <= 0f)
        {
            ThrowBombTowardsPlayer();
            cooldownTimer = timeBreak;
        }
    }

    private void ThrowBombTowardsPlayer()
    {
        if (bombPrefab == null) return;

        Vector3 spawnPos = throwPoint != null ? throwPoint.position : transform.position;

        float direction = (player.transform.position.x > transform.position.x) ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);

        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        GameObject bombInstance = Instantiate(bombPrefab, spawnPos, Quaternion.identity);

        Collider2D pigCollider = GetComponent<Collider2D>();
        Collider2D bombCollider = bombInstance.GetComponent<Collider2D>();

        if (pigCollider != null && bombCollider != null)
        {
            Physics2D.IgnoreCollision(pigCollider, bombCollider);
        }

        Rigidbody2D bombRb = bombInstance.GetComponent<Rigidbody2D>();
        if (bombRb != null)
        {
            Vector2 targetDirection = (player.transform.position - spawnPos).normalized;
            Vector2 throwVector = targetDirection * throwForce + Vector2.up * throwUpForce;

            bombRb.AddForce(throwVector, ForceMode2D.Impulse);
            bombRb.AddTorque(-direction * 3f, ForceMode2D.Impulse);
        }

        BoomController boomController = bombInstance.GetComponent<BoomController>();
        if (boomController != null)
        {
            boomController.ActivateBoom();
        }
    }
}