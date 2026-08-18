using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Toggle muteToggle;
    [SerializeField] private GameObject settingsPanel; // Panel Setting để ẩn/hiện

    private float tempMusicVol;
    private float tempSFXVol;
    private bool tempIsMuted;

    private void OnEnable()
    {
        // Đọc giá trị đã lưu
        tempMusicVol = PlayerPrefs.GetFloat("MusicVolume", 1f);
        tempSFXVol = PlayerPrefs.GetFloat("SFXVolume", 1f);
        tempIsMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;

        // Cập nhật lên UI
        if (musicSlider) musicSlider.value = tempMusicVol;
        if (sfxSlider) sfxSlider.value = tempSFXVol;
        if (muteToggle) muteToggle.isOn = tempIsMuted;

        // Đăng ký sự kiện lắng nghe thay đổi
        musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.AddListener(OnSFXSliderChanged);
        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
    }

    private void OnDisable()
    {
        musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        sfxSlider.onValueChanged.RemoveListener(OnSFXSliderChanged);
        muteToggle.onValueChanged.RemoveListener(OnMuteToggleChanged);
    }

    // Khi người chơi kéo slider
    public void OnMusicSliderChanged(float val)
    {
        tempMusicVol = val;
        if (AudioManager.Instance != null) AudioManager.Instance.SetMusicVolume(val);
    }

    public void OnSFXSliderChanged(float val)
    {
        tempSFXVol = val;
        if (AudioManager.Instance != null) AudioManager.Instance.SetSFXVolume(val);
    }

    public void OnMuteToggleChanged(bool isMuted)
    {
        tempIsMuted = isMuted;
        if (AudioManager.Instance != null) AudioManager.Instance.SetMute(isMuted);
    }

    // Nút 1: LƯU VÀ THOÁT
    public void Btn_SaveAndExit()
    {
        PlayerPrefs.SetFloat("MusicVolume", tempMusicVol);
        PlayerPrefs.SetFloat("SFXVolume", tempSFXVol);
        PlayerPrefs.SetInt("IsMuted", tempIsMuted ? 1 : 0);
        PlayerPrefs.Save();

        CloseSettings();
    }

    // Nút 2: MẶC ĐỊNH (RESET)
    public void Btn_ResetToDefault()
    {
        tempMusicVol = 1f;
        tempSFXVol = 1f;
        tempIsMuted = false;

        musicSlider.value = tempMusicVol;
        sfxSlider.value = tempSFXVol;
        muteToggle.isOn = tempIsMuted;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.SetMusicVolume(tempMusicVol);
            AudioManager.Instance.SetSFXVolume(tempSFXVol);
            AudioManager.Instance.SetMute(tempIsMuted);
        }
    }

    // Nút 3: HỦY (Cancel)
    public void Btn_Cancel()
    {
        // Khôi phục lại cài đặt cũ trước khi bấm mở menu
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.LoadAudioSettings();
        }
        CloseSettings();
    }

    private void CloseSettings()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false); // Đóng bảng Setting quay về Main Menu
        }
    }
}