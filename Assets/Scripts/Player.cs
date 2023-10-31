using UnityEngine;
using UnityEngine.EventSystems;

public class Player : MonoBehaviour {


    public new Camera camera;
    public Rigidbody body;
    public SpringJoint springJoint;
    public PlayerSettings playerSettings;

    public bool isAlive = true;




    private void Awake() {

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }



    private void Update() {
        isAlive = !Input.GetKey(KeyCode.LeftAlt);

        body.freezeRotation = isAlive;
    }

}
