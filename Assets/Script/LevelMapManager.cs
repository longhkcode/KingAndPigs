using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelMapManager : MonoBehaviour
{
    [Header("List Flags in this Map")]
    public LevelButton[] levelButtons; // Kéo thả danh sách các cờ trong Scene vào đây

    private void Start()
    {
        RefreshMap();
    }

    public void RefreshMap()
    {
        foreach (var flag in levelButtons)
        {
            if (flag != null)
            {
                flag.SetupButton();
            }
        }
    }

    // Chuyển sang Map 2 (Levels_CR2) khi bấm nút Arrow
    public void GoToNextMap()
    {
        SceneManager.LoadScene("Levels_CR2");
    }

    public void BackToMap1()
    {
        SceneManager.LoadScene("Levels_CR1");
    }


    // Về Menu chính khi bấm nút Home
    public void GoToHome()
    {
        SceneManager.LoadScene("Menu");
    }
}