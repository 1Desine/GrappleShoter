using UnityEngine;

public class InputManager : MonoBehaviour {
    static public InputManager Instance { get; private set; }

    PlayerInputActions playerInputActions;

    [SerializeField] PlayerSettingsSO playerSettingsSO;


    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }

    private void FixedUpdate() {
        if (Input.GetKey(KeyCode.Minus)) playerSettingsSO.sensitivity -= 0.05f * Time.fixedDeltaTime;
        if (Input.GetKey(KeyCode.Equals)) playerSettingsSO.sensitivity += 0.05f * Time.fixedDeltaTime;
        if(playerSettingsSO.sensitivity < 0) playerSettingsSO.sensitivity = 0; 
        InputDebugUI.Instance.SetMouseSensText(playerSettingsSO.sensitivity);
    }


    public Vector2 GetLookVector2Delta() {
        return playerInputActions.Player.Look.ReadValue<Vector2>() * playerSettingsSO.sensitivity;
    }

    public Vector2 GetMoveVector2() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }


}
