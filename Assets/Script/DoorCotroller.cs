using System.Collections;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorController : MonoBehaviour
{
    private Animator anim;
    private Collider2D doorCollider;

    private bool isOpen = false;
    private bool isTransitioning = false;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        doorCollider = GetComponent<Collider2D>();

        if (doorCollider != null)
        {
            doorCollider.isTrigger = true;
        }
    }

    private void Start()
    {
        StartCoroutine(OpeningAnimationAtStart());
    }

    private IEnumerator OpeningAnimationAtStart()
    {
        yield return new WaitForSeconds(1.5f);

        if (anim != null)
        {
            anim.SetTrigger("Close");
        }

        isOpen = false;
    }

    public void OpenDoor()
    {
        isOpen = true;

        if (anim != null)
        {
            anim.SetTrigger("Open");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isOpen && !isTransitioning && other.CompareTag("Player"))
        {
            StartCoroutine(CloseDoorAndNextScene(other.gameObject));
        }
    }

    private IEnumerator CloseDoorAndNextScene(GameObject playerObj)
    {
        isTransitioning = true;

        // 1. Tự động lấy số level từ tên Scene hiện tại (VD: "Map1" -> 1, "Map2" -> 2)
        int currentLevelIndex = GetCurrentLevelIndex();

        // 2. Mở khóa Level tiếp theo vào PlayerPrefs
        int currentUnlocked = PlayerPrefs.GetInt("UnlockedLevel", 1);
        if (currentLevelIndex >= currentUnlocked)
        {
            PlayerPrefs.SetInt("UnlockedLevel", currentLevelIndex + 1);
            PlayerPrefs.Save();
        }

        // 3. Chuyển Player về tâm cửa, tắt vật lý và chạy Animation Player_DoorIn
        PlayerController player = playerObj.GetComponent<PlayerController>();
        if (player != null)
        {
            yield return StartCoroutine(player.PlayDoorInRoutine(transform.position));
        }

        // 4. Đóng cửa lại
        if (anim != null)
        {
            anim.SetTrigger("Close");
        }

        yield return new WaitForSeconds(0.5f);

        // 5. Load về Scene chọn level
        SceneManager.LoadScene("Levels_CR1");
    }

    /// <summary>
    /// Lấy chỉ số Level tự động từ tên của Scene hiện tại
    /// </summary>
    private int GetCurrentLevelIndex()
    {
        string sceneName = SceneManager.GetActiveScene().name; // Ví dụ lấy được "Map1" hoặc "Map12"
        string numbersOnly = Regex.Replace(sceneName, @"[^\d]", ""); // Lọc bỏ chữ, chỉ giữ lại số

        if (int.TryParse(numbersOnly, out int levelIndex))
        {
            return levelIndex;
        }

        return 1; // Mặc định trả về 1 nếu tên Scene không chứa số
    }
}