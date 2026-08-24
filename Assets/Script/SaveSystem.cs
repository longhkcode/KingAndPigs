using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public int totalDiamonds = 0;
    public int unlockedLevel = 1;
}

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "player_data.json");

    // Hàm lưu dữ liệu
    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log("Đã lưu dữ liệu vào: " + SavePath);
    }

    // Hàm đọc dữ liệu
    public static SaveData LoadGame()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }

        // Lần đầu chơi thì trả về dữ liệu mặc định
        return new SaveData();
    }
}