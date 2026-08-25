using System.Collections;
using UnityEngine;

public class SpamEnemy : MonoBehaviour
{
    [Header("Enemy Prefabs (4 loại Pig)")]
    public GameObject pigNormalPrefab; // PigNormal
    public GameObject pigBoxPrefab;    // PigBox
    public GameObject pigBombPrefab;   // PigBomb
    public GameObject pigCanonPrefab;  // PigCanon

    [Header("Item Prefabs")]
    public GameObject healthPrefab;    // Prefab Máu
    [Range(0, 100)] public float healthSpawnChance = 15f; // Tỉ lệ rớt máu (15%)

    [Header("Spawn Settings")]
    public Transform[] spawnPositions; // Danh sách vị trí cố định để spawn (dành cho Box, Bomb, Canon)
    public float spawnInterval = 1f;    // Cứ 1 giây spawn 1 con
    public bool canSpawn = true;

    [Header("Map Bounds for PigNormal Spawn")]
    // Tọa độ phạm vi ngẫu nhiên trên map dành riêng cho PigNormal
    public float minX = -12f;
    public float maxX = 50f;
    public float minY = -50f;
    public float maxY = 12f;

    private void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (canSpawn)
        {
            yield return new WaitForSeconds(spawnInterval);

            // Kiểm tra: Nếu CHƯA xuất hiện Boss thì mới spawn Enemy
            if (GameManagerMap25.Instance == null || !GameManagerMap25.Instance.IsBossSpawned())
            {
                SpawnRandomEnemy();
            }

            // Máu vẫn luôn được spawn bình thường ngay cả khi đã có Boss
            TrySpawnHealth();
        }
    }

    private void SpawnRandomEnemy()
    {
        float randomPercent = Random.Range(0f, 100f);

        // --- 1. PIGNORMAL (60% tỉ lệ: 0 -> 60) ---
        if (randomPercent < 60f)
        {
            if (pigNormalPrefab != null)
            {
                Vector3 randomMapPos = new Vector3(
                    Random.Range(minX, maxX),
                    Random.Range(minY, maxY),
                    0f
                );

                GameObject pig = Instantiate(pigNormalPrefab, randomMapPos, Quaternion.identity);

                PigNormal pigScript = pig.GetComponent<PigNormal>();
                if (pigScript != null)
                {
                    pigScript.ForceChase();
                }
            }
        }
        // --- 2. XỬ LÝ 3 LOẠI PIG CÒN LẠI (40% còn lại: 60 -> 100) ---
        else
        {
            if (spawnPositions == null || spawnPositions.Length == 0) return;

            int randomPosIndex = Random.Range(0, spawnPositions.Length);
            Transform spawnPoint = spawnPositions[randomPosIndex];

            if (spawnPoint == null) return;

            GameObject prefabToSpawn = null;

            if (randomPercent >= 60f && randomPercent < 80f)
            {
                prefabToSpawn = pigBoxPrefab;
            }
            else if (randomPercent >= 80f && randomPercent < 90f)
            {
                prefabToSpawn = pigBombPrefab;
            }
            else
            {
                prefabToSpawn = pigCanonPrefab;
            }

            if (prefabToSpawn != null)
            {
                Instantiate(prefabToSpawn, spawnPoint.position, Quaternion.identity);
            }
        }
    }

    private void TrySpawnHealth()
    {
        if (healthPrefab == null) return;

        if (Random.Range(0f, 100f) <= healthSpawnChance)
        {
            Vector3 healthSpawnPos;

            if (Random.value > 0.5f || spawnPositions == null || spawnPositions.Length == 0)
            {
                healthSpawnPos = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), 0f);
            }
            else
            {
                Transform randomPoint = spawnPositions[Random.Range(0, spawnPositions.Length)];
                healthSpawnPos = randomPoint != null ? randomPoint.position : Vector3.zero;
            }

            Instantiate(healthPrefab, healthSpawnPos, Quaternion.identity);
        }
    }
}