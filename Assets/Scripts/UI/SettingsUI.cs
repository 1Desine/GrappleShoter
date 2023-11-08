using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUI : MonoBehaviour {
    static public SettingsManager Instance { get; private set; }

    [SerializeField] TextMeshProUGUI fovValue;
    [SerializeField] Button fovMinusButton;
    [SerializeField] Button fovPlusButton;
    [SerializeField] TextMeshProUGUI mouseSensitivityValue;
    [SerializeField] Button mouseSensitivityMinusButton;
    [SerializeField] Button mouseSensitivityPlusButton;



    private void Start() {
        // Mouse sensitivity
        mouseSensitivityMinusButton.onClick.AddListener(() => {
            SettingsManager.Instance.ChangeSensitivity(-1);
            UpdateSettingsValuesVisual();
        });
        mouseSensitivityPlusButton.onClick.AddListener(() => {
            SettingsManager.Instance.ChangeSensitivity(1);
            UpdateSettingsValuesVisual();
        });
        // FOV
        fovMinusButton.onClick.AddListener(() => {
            SettingsManager.Instance.ChangeFov(-1);
            UpdateSettingsValuesVisual();
        });
        fovPlusButton.onClick.AddListener(() => {
            SettingsManager.Instance.ChangeFov(1);
            UpdateSettingsValuesVisual();
        });

        UpdateSettingsValuesVisual();
        SettingsManager.Instance.OnShowSettings += Show;
        SettingsManager.Instance.OnHideSettings += Hide;
        Hide();
    }

    void UpdateSettingsValuesVisual() {
        mouseSensitivityValue.text = SettingsManager.Instance.playerSettingsSO.sensitivity.ToString("F2");
        fovValue.text = SettingsManager.Instance.playerSettingsSO.fov.ToString();
    }


    public void Show() {
        gameObject.SetActive(true);
    }
    public void Hide() {
        gameObject.SetActive(false);
    }




}
