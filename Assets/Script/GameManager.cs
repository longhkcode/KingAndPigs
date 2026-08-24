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

    [Header("Panels UI")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject loseUI; // Thêm Lose UI vào đây
    
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

        // Ẩn các Panel khi bắt đầu
        if (pauseUI != null) pauseUI.SetActive(false);
        if (loseUI != null) loseUI.SetActive(false);

        // Gán sự kiện cho nút Resume trong Pause Panel nếu có
        if (pauseUI != null)
        {
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

        // Đọc số Kim Cương đã lưu
        SaveData data = SaveSystem.LoadGame();
        score = data.totalDiamonds;
        UpdateUI();

        // Spawn Player
        SpawnPlayer();

        // Đếm Enemy
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        Debug.Log("Tổng số quái trong màn: " + enemyCount);

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

        Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("Player đã được Spawn!");
    }

    public void AddScore(int value)
    {
        score += value;

        SaveData data = SaveSystem.LoadGame();
        data.totalDiamonds = score;
        SaveSystem.SaveGame(data);

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
        enemyCount = Mathf.Max(enemyCount, 0);

        Debug.Log("Còn lại: " + enemyCount + " quái");

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
    
    // --- XỬ LÝ GAMEOVER / LOSE ---
    public void GameOver()
    {
        if (loseUI != null)
        {
            loseUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManager: Chưa gán Lose UI trong Inspector!");
        }

        Time.timeScale = 0f; // Dừng thời gian toàn bộ game
    }

    // --- XỬ LÝ PAUSE ---
    public void PauseGame()
    {
        Time.timeScale = 0f;

        if (pauseUI != null)
        {
            pauseUI.SetActive(true);
        }
    }

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
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void ExitToMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Menu");
    }
}