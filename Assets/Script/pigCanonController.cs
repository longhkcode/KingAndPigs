using UnityEngine;

public class pigCanonController : EnemyController
{
    [Header("Settings")]
    [SerializeField] private CanonController canon;     // Kéo pháo (Child) vào đây
    [SerializeField] private float attackDistance = 8f; // Tầm phát hiện Player
    [SerializeField] private float timeBreak = 3f;      // Thời gian hồi giữa 2 lần bắn
    
    private float coolDownTimer;

    protected override void Start()
    {
        base.Start();
        if (canon == null)
        {
            canon = GetComponentInChildren<CanonController>();
        }
    }

    private void Update()
    {
        if (coolDownTimer > 0)
        {
            coolDownTimer -= Time.deltaTime;
        }

        if (EnsurePlayerReference() == null || canon == null) return;

        float distanceToPlayer = Vector3.Distance(player.transform.position, canon.transform.position);
        if (distanceToPlayer <= attackDistance && coolDownTimer <= 0)
        {
            IgniteAndShoot();
            coolDownTimer = timeBreak;
        }
    }
    
    private void IgniteAndShoot()
    {
        // 1. Lật localScale của Pig -> CẢ PHÁO (CHILD) SẼ TỰ ĐỘNG LẬT THEO
        float direction = (player.transform.position.x < transform.position.x) ? 1f : -1f;
        transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y, transform.localScale.z);

        // 2. Kích hoạt Animation châm lửa của Pig
        if (anim != null)
        {
            anim.SetTrigger("Attack");
        }

        // 3. Ra lệnh cho pháo bắn
        canon.Fire(player.transform.position);
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.orange;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}