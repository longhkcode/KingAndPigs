using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManagerMap25 : MonoBehaviour
{
    public static GameManagerMap25 Instance { get; private set; }

    [Header("Score & UI")]
    [SerializeField] private Text dimondText;
    private int score = 0;

    [Header("Player Spawn")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnPoint;

    [Header("Door & Scenes")]
    [SerializeField] private DoorController exitDoor;

    [Header("Map25 - Phase 1: Normal Enemies")]
    [SerializeField] private int requiredKillsForBoss = 2;
    private int killedEnemyCount = 0;

    [Header("Map25 - Phase 2: Boss & Bomb Rain")]
    [SerializeField] private GameObject bossPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    [SerializeField] private GameObject bombPrefab;
    [SerializeField] private List<Transform> bombRainPoints = new List<Transform>();
    [SerializeField] private float bombRainInterval = 5f;

    private GameObject currentBossInstance;
    private bool isBossSpawned = false;
    private Coroutine bombRainCoroutine;

    [Header("Panels UI")]
    [SerializeField] private GameObject pauseUI;
    [SerializeField] private GameObject loseUI; // Thêm Lose UI cho Map 25

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
        Time.timeScale = 1f;

        // Ẩn các Panel UI khi bắt đầu game
        if (pauseUI != null) pauseUI.SetActive(false);
        if (loseUI != null) loseUI.SetActive(false);

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

        SaveData data = SaveSystem.LoadGame();
        score = data.totalDiamonds;
        UpdateUI();

        SpawnPlayer();
    }

    private void SpawnPlayer()
    {
        if (playerPrefab != null && spawnPoint != null)
        {
            Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        }
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
        if (isBossSpawned) return;

        killedEnemyCount++;
        Debug.Log("Số quái đã hạ: " + killedEnemyCount + "/" + requiredKillsForBoss);

        if (killedEnemyCount >= requiredKillsForBoss)
        {
            SpawnBossPhase();
        }
    }

    private void SpawnBossPhase()
    {
        isBossSpawned = true;

        // Dọn sạch toàn bộ quái thường có Tag "Enemy" trên bản đồ
        GameObject[] remainingEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject enemy in remainingEnemies)
        {
            Destroy(enemy);
        }

        // Triệu hồi Boss
        if (bossPrefab != null && bossSpawnPoint != null)
        {
            currentBossInstance = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        }

        // Bắt đầu mưa bom
        if (bombRainCoroutine == null)
        {
            bombRainCoroutine = StartCoroutine(StartBombRain());
        }
    }

    private IEnumerator StartBombRain()
    {
        while (isBossSpawned)
        {
            yield return new WaitForSeconds(bombRainInterval);

            if (bombPrefab != null && bombRainPoints.Count > 0)
            {
                foreach (Transform point in bombRainPoints)
                {
                    if (point != null)
                    {
                        GameObject bomb = Instantiate(bombPrefab, point.position, Quaternion.identity);

                        BoomController boomCtrl = bomb.GetComponent<BoomController>();
                        if (boomCtrl != null)
                        {
                            boomCtrl.ActivateCanonBoom();
                        }
                    }
                }
            }
        }
    }

    public void BossKilled()
    {
        if (bombRainCoroutine != null)
        {
            StopCoroutine(bombRainCoroutine);
        }

        OpenDoor();
    }

    private void OpenDoor()
    {
        if (exitDoor != null)
        {
            exitDoor.OpenDoor();
        }
    }

    // --- THÊM HÀM GAMEOVER ---
    public void GameOver()
    {
        // Dừng coroutine mưa bom nếu đang chạy
        if (bombRainCoroutine != null)
        {
            StopCoroutine(bombRainCoroutine);
        }

        // Hiển thị Lose UI
        if (loseUI != null)
        {
            loseUI.SetActive(true);
        }
        else
        {
            Debug.LogWarning("GameManagerMap25: Chưa gán Lose UI trong Inspector!");
        }

        // Dừng thời gian toàn bộ game
        Time.timeScale = 0f;
    }

    public void LoadWinGameScene()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("WinGame");
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (pauseUI != null) pauseUI.SetActive(true);
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (pauseUI != null) pauseUI.SetActive(false);
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

    public bool IsBossSpawned()
    {
        return isBossSpawned;
    }
}