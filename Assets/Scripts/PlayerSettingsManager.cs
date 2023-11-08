using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingsManager : MonoBehaviour {
    static public PlayerSettingsManager Instance { get; private set; }


    public PlayerSettingsSO playerSettingsSO;

    public Action OnFovChanged = () => { };



    private void Awake() {
        Instance = this;
    }

    private void Update() {
        float changeFov = 0;
        if (Input.GetKeyDown(KeyCode.LeftBracket)) changeFov++;
        if (Input.GetKeyDown(KeyCode.RightBracket)) changeFov--;
        if (changeFov != 0) {
            playerSettingsSO.fov = Mathf.RoundToInt(Mathf.Clamp(playerSettingsSO.fov + changeFov * 5, 5, 180));
            OnFovChanged();
        }
    }
    private void FixedUpdate() {
        if (Input.GetKey(KeyCode.Minus)) playerSettingsSO.sensitivity -= 0.05f * Time.fixedDeltaTime;
        if (Input.GetKey(KeyCode.Equals)) playerSettingsSO.sensitivity += 0.05f * Time.fixedDeltaTime;
        if (playerSettingsSO.sensitivity < 0) playerSettingsSO.sensitivity = 0;
        InputDebugUI.Instance.SetMouseSensText(playerSettingsSO.sensitivity);

    }



}
