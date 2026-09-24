using UnityEngine;

public class PlayerCanvasSetup : MonoBehaviour
{
    private void Start()
    {
        Canvas canvas = GetComponent<Canvas>();
        if (canvas != null)
        {
            GameObject mainCamObj = GameObject.FindWithTag("MainCamera");
            
            if (mainCamObj != null)
            {
                canvas.worldCamera = mainCamObj.GetComponent<Camera>();
            }
            else
            {
                Debug.LogError("PlayerCanvasSetup: Không tìm thấy GameObject nào có tag 'MainCamera'!");
            }

            // Đảm bảo Order in Layer cao để UI không bị che bởi Tilemap
            canvas.sortingOrder = 2;
        }
    }
}