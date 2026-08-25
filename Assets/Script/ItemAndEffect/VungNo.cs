using System.Collections.Generic;
using UnityEngine;

public class VungNo : MonoBehaviour
{
    [SerializeField] private float damage = 10f;
    
    // danh sach doi tuong trung don r
    private HashSet<Collider2D> hitObjects = new HashSet<Collider2D>();

    private void OnEnable()
    {
        hitObjects.Clear(); // xoa danh sach de co boom ms 
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (hitObjects.Contains(collision)) return; // trung r thi bo qua

        if (collision.tag == "Player")
        {
            var player = collision.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(damage);
                hitObjects.Add(collision);
            }
        }
    }
    
}
