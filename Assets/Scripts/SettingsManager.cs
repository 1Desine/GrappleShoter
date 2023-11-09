using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SettingsManager : MonoBehaviour {
    static public SettingsManager Instance { get; private set; }


    public PlayerSettingsSO playerSettingsSO;

    bool settingMenuActive = false;
    public Action OnShowSettings = () => { };
    public Action OnHideSettings = () => { };

    public Action OnFovChanged = () => { };
    public Action OnSensitivityChanged = () => { };



    private void Awake() {
        Instance = this;

        HideCursor();
    }
    void HideCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void ShowCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void FixedUpdate() {
        if (Input.GetKey(KeyCode.Minus)) playerSettingsSO.sensitivity -= 0.05f * Time.fixedDeltaTime;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            settingMenuActive = !settingMenuActive;
            if (settingMenuActive) {
                OnShowSettings();
                ShowCursor();
                InputManager.Instance.DisablePlayerInput();
            }
            else {
                OnHideSettings();
                HideCursor();
                InputManager.Instance.EnablePlayerInput();
            }
        }
    }

    public void ChangeFov(int add) {
        playerSettingsSO.fov = Mathf.RoundToInt(Mathf.Clamp(playerSettingsSO.fov + add * 5, 5, 180));
        OnFovChanged();
    }
    public void ChangeSensitivity(int add) {
        playerSettingsSO.sensitivity += add * 0.01f;
        OnSensitivityChanged();
        if (playerSettingsSO.sensitivity < 0) playerSettingsSO.sensitivity = 0;
    }


}
