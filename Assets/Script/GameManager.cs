using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Score & UI")]
    [SerializeField] private Text dimondText;
    private int score = 0;

    [Header("Player Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Door & Enemies")]
    [SerializeField] private DoorController exitDoor;

    private int enemyCount;

    [Header("Pause UI")]
    [SerializeField] private GameObject pauseUI;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        // Đảm bảo game chạy bình thường
        Time.timeScale = 1f;

        // Ẩn Pause Panel
        if (pauseUI != null)
        {
            pauseUI.SetActive(false);
        }
        
        if (pauseUI != null)
        {
            pauseUI.SetActive(false);

            // Tìm nút Resume bên trong pauseUI và tự gán sự kiện Click
            Transform resumeBtnTransform = pauseUI.transform.Find("Resume");
            if (resumeBtnTransform != null)
            {
                Button resumeBtn = resumeBtnTransform.GetComponent<Button>();
                if (resumeBtn != null)
                {
                    resumeBtn.onClick.RemoveAllListeners();
                    resumeBtn.onClick.AddListener(ResumeGame);
                }
            }
        }

        // ĐỌC SỐ KIM CƯƠNG ĐÃ LƯU TỪ PLAYERPREFS (Mặc định là 0 nếu mới bắt đầu game)
        score = PlayerPrefs.GetInt("TotalDiamonds", 0);
        UpdateUI();

        // Spawn Player
        SpawnPlayer();

        // Đếm Enemy
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;

        Debug.Log("Tổng số quái trong màn: " + enemyCount);

        // Nếu không có quái thì mở cửa luôn
        if (enemyCount <= 0)
        {
            OpenDoor();
        }
    }
    
    private void SpawnPlayer()
    {
        if (playerPrefab == null)
        {
            Debug.LogError("GameManager: Chưa gán Player Prefab!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("GameManager: Chưa gán Spawn Point!");
            return;
        }

        Instantiate(
            playerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        Debug.Log("Player đã được Spawn!");
    }

    public void AddScore(int value)
    {
        score += value;

        // LƯU NGAY SỐ KIM CƯƠNG MỚI VÀO PLAYERPREFS
        PlayerPrefs.SetInt("TotalDiamonds", score);
        PlayerPrefs.Save();

        UpdateUI();
    }
    
    public void UpdateUI()
    {
        if (dimondText != null)
        {
            dimondText.text = score.ToString();
        }
    }

    public void EnemyKilled()
    {
        enemyCount--;

        // Không cho số lượng âm
        enemyCount = Mathf.Max(enemyCount, 0);

        Debug.Log("Còn lại: " + enemyCount + " quái");

        // Giết hết quái → mở cửa
        if (enemyCount <= 0)
        {
            OpenDoor();
        }
    }
    
    private void OpenDoor()
    {
        if (exitDoor != null)
        {
            exitDoor.OpenDoor();

            Debug.Log("Đã giết hết quái - Cửa đã mở!");
        }
        else
        {
            Debug.LogWarning("GameManager: Chưa gán Exit Door!");
        }
    }
    
    // --- THAY ĐỔI XỬ LÝ PAUSE TẠI ĐÂY ---
    public void PauseGame()
    {
        Time.timeScale = 0f;

        if (pauseUI != null)
        {
            pauseUI.SetActive(true);
        }
        else
        {
            Debug.LogError("GameManager: Chưa gán Pause UI trong Inspector!");
        }
    }

    // Hàm bổ trợ giúp nút Pause trong Prefab Player gọi dễ dàng qua Singleton
    public void TogglePauseFromButton()
    {
        if (Instance != null)
        {
            Instance.PauseGame();
        }
    }
    
    public void ResumeGame()
    {
        Time.timeScale = 1f;

        if (pauseUI != null)
        {
            pauseUI.SetActive(false);
        }
    }
    
    public void RestartGame()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().buildIndex
        );
    }
    
    public void ExitToMenu()
    {
        // Reset TimeScale
        Time.timeScale = 1f;

        // Tên Scene Menu của bạn
        SceneManager.LoadScene("Menu");
    }
}