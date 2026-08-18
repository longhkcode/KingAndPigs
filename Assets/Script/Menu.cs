using UnityEngine;

public class Menu : MonoBehaviour
{
    [Header("Plot Panels")]
    [SerializeField] private GameObject plot1;
    [SerializeField] private GameObject plot2;

    private void Start()
    {
        // Ẩn cả 2 plot khi bắt đầu
        CloseAllPlots();
    }

    // Nút PLOT ngoài Menu
    public void OpenPlot1()
    {
        if (plot1 != null) plot1.SetActive(true);
        if (plot2 != null) plot2.SetActive(false);
    }

    // Nút TIẾP TỤC ở Plot 1 -> Mở Plot 2, Tắt Plot 1
    public void OpenPlot2()
    {
        if (plot1 != null) plot1.SetActive(false);
        if (plot2 != null) plot2.SetActive(true);
    }

    // Nút TRỞ VỀ ở Plot 2 -> Mở Plot 1, Tắt Plot 2
    public void BackToPlot1()
    {
        if (plot1 != null) plot1.SetActive(true);
        if (plot2 != null) plot2.SetActive(false);
    }

    // Nút TRỜ VỀ / Nút X ở Plot 1, HOẶC Nút TIẾP TỤC / Nút X ở Plot 2
    public void CloseAllPlots()
    {
        if (plot1 != null) plot1.SetActive(false);
        if (plot2 != null) plot2.SetActive(false);
    }

    public GameObject settingPanel;
    public void OpenSettings()
    {
        if(settingPanel != null) settingPanel.SetActive(true);
    }
    
}