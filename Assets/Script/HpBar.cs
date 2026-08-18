using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HpBar : MonoBehaviour
{
    public Image fillBar;

    // Giả sử vùng chứa tim chỉ chiếm từ minFill đến maxFill trên bức ảnh
    [Range(0f, 1f)] public float minFill = 0.05f; 
    [Range(0f, 1f)] public float maxFill = 0.95f; 

    public void updateBar(float curHealth, float maxHealth)
    {
        if (maxHealth <= 0) return;

        float pct = curHealth / maxHealth; // Tỉ lệ máu thực tế (0.0 đến 1.0)
        
        // Map tỉ lệ máu vào đúng khoảng hiển thị của dãy tim
        fillBar.fillAmount = Mathf.Lerp(minFill, maxFill, pct);
    }
}