using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {

    public bool isAlive = true;


    private Rigidbody body;

    private void Awake() {
        body = GetComponent<Rigidbody>();

        Cursor.lockState = CursorLockMode.Locked;
    }



    private void Update() {
        isAlive = !Input.GetKey(KeyCode.LeftControl);

        body.freezeRotation = isAlive;
    }

}
