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
    }

    private void FixedUpdate() {
        if (Input.GetKey(KeyCode.Minus)) playerSettingsSO.sensitivity -= 0.05f * Time.fixedDeltaTime;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            if (settingMenuActive) OnHideSettings();
            else OnShowSettings();
            settingMenuActive = !settingMenuActive;
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
