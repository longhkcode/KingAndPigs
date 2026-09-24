using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class PauseButton : MonoBehaviour
{
    private Button btn;

    private void Awake()
    {
        btn = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnPauseClicked); // Tránh trùng lặp listener
            btn.onClick.AddListener(OnPauseClicked);
        }
    }

    private void OnDisable()
    {
        if (btn != null)
        {
            btn.onClick.RemoveListener(OnPauseClicked);
        }
    }

    private void OnPauseClicked()
    {
        Debug.Log("Đã bấm vào nút Pause!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
        else
        {
            Debug.LogWarning("PauseButton: Chưa tìm thấy GameManager Instance!");
        }
    }
}