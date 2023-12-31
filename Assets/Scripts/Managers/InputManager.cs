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
    public void DisablePlayerInput() => playerInputActions.Disable();
    public void EnablePlayerInput() => playerInputActions.Enable();


    public Vector2 GetLookVector2Delta() {
        return playerInputActions.Player.Look.ReadValue<Vector2>() * SettingsManager.Instance.playerSettingsSO.sensitivity;
    }

    public Vector2 GetMoveVector2() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }


}
