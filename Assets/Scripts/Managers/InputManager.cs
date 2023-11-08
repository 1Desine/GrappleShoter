using System;
using UnityEngine;

public class InputManager : MonoBehaviour {
    static public InputManager Instance { get; private set; }

    PlayerInputActions playerInputActions;


    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }


    public Vector2 GetLookVector2Delta() {
        return playerInputActions.Player.Look.ReadValue<Vector2>() * PlayerSettingsManager.Instance.playerSettingsSO.sensitivity;
    }

    public Vector2 GetMoveVector2() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }


}
