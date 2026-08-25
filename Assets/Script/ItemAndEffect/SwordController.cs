using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    [SerializeField] private float damage = 10f; 

    // Danh sách lưu các đối tượng đã trúng đòn trong 1 lần chém
    private HashSet<Collider2D> hitObjects = new HashSet<Collider2D>();

    // Hàm này được gọi mỗi khi bật/tắt Collider chém (khi bắt đầu đòn đánh mới)
    private void OnEnable()
    {
        hitObjects.Clear(); // Xóa danh sách cũ để chuẩn bị cho cú chém mới
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Nếu đối tượng này đã bị dính đòn trong cú chém này rồi -> Bỏ qua!
        if (hitObjects.Contains(collision)) return;

        // 1. Chém Enemy
        if (collision.CompareTag("Enemy") || collision.CompareTag("BossEnemy"))
        {
            var enemy = collision.GetComponent<EnemyController>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage); 
                hitObjects.Add(collision); // Đánh dấu đã chém trúng con này rồi
            }
        }

        // 2. Chém Thùng
        if (collision.CompareTag("Box"))
        {
            var box = collision.GetComponent<BoxController>();
            if (box != null)
            {
                box.TakeDamage(damage);
                hitObjects.Add(collision);
            }
        }
    }
}