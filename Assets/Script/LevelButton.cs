using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LevelButton : MonoBehaviour
{
    [Header("Level Info")]
    public int levelIndex; // ID level tương ứng (Level 1 - 15 thuộc Map 1, 16 - 30 thuộc Map 2)

    [Header("Flag Sprites")]
    public Sprite redFlag;   // Lá cờ đỏ (Level hiện tại chưa hoàn thành)
    public Sprite greenFlag; // Lá cờ xanh (Level đã qua bài)
    public Sprite greyFlag;  // Lá cờ xám (Level bị khóa)

    private Image flagImage;
    private Button button;

    private void Awake()
    {
        flagImage = GetComponent<Image>();
        button = GetComponent<Button>();
    }

    public void SetupButton()
    {
        int unlockedLevel = PlayerPrefs.GetInt("UnlockedLevel", 1);

        flagImage.color = Color.white; // Giữ màu gốc không bị mờ

        if (levelIndex < unlockedLevel)
        {
            flagImage.sprite = greenFlag;
            button.interactable = true;
        }
        else if (levelIndex == unlockedLevel)
        {
            flagImage.sprite = redFlag;
            button.interactable = true;
        }
        else
        {
            flagImage.sprite = greyFlag;
            button.interactable = false;
        }

        // Đặt kích thước ảnh về chuẩn kích thước gốc của Sprite mới
        flagImage.SetNativeSize();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnLevelSelected);
    }

    private void OnLevelSelected()
    {
        SceneManager.LoadScene("Map" + levelIndex);
    }
}