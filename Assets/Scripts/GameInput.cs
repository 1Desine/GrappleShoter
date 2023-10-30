using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : MonoBehaviour {
    static public GameInput Instance { get; private set; }

    PlayerInputActions playerInputActions;


    private void Awake() {
        Instance = this;

        playerInputActions = new PlayerInputActions();
        playerInputActions.Enable();
    }





    public Vector2 GetLookVector2Delta() {
        return playerInputActions.Player.Look.ReadValue<Vector2>();
    }

    public Vector2 GetMoveVector2() {
        return playerInputActions.Player.Move.ReadValue<Vector2>();
    }


}
