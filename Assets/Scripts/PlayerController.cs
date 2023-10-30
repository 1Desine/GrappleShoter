using UnityEngine;

public class PlayerController : MonoBehaviour {

    Player player;

    [SerializeField] new Camera camera;
    GrapplingHook grapplingHook;
    [SerializeField] PlayerSettings playerSettings;


    [SerializeField] float moveForce;
    [SerializeField] AnimationCurve moveForceCurve;
    [SerializeField] float maxSpeedByWalk;


    Rigidbody body;
    GameInput gameInput;

    Vector2 lookVector;

    private void Awake() {
        body = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        grapplingHook = GetComponent<GrapplingHook>();
    }

    private void Start() {
        gameInput = GameInput.Instance;
    }
    private void Update() {
        if (player.isAlive) {
            Looking();
            Movement();
        }

        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(camera.transform.position, camera.transform.forward, out RaycastHit hit);

            grapplingHook.SetAnchorPosition(hit.point);
        }
        if (Input.GetMouseButtonUp(0)) {
            grapplingHook.ResetAnchorPosition();
        }

        if (Input.GetMouseButtonDown(1)) {
            grapplingHook.PullUp();
        }
    }

    void Movement() {
        Vector2 moveInput = gameInput.GetMoveVector2();
        Vector3 desiredMoveDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        float dotVelocityByDesiredDir = Mathf.Max(0, 1 - Vector3.Dot(body.velocity.normalized, desiredMoveDir));
        //Debug.Log("dotVelocityByDesiredDir " + dotVelocityByDesiredDir);

        float evaluatedForce = moveForceCurve.Evaluate(dotVelocityByDesiredDir);

        body.AddForce(desiredMoveDir * evaluatedForce * moveForce * Time.deltaTime);
    }
    void Looking() {
        Vector2 lookInput = gameInput.GetLookVector2Delta() * playerSettings.sensitivity;
        lookVector += lookInput;

        transform.eulerAngles = Vector3.up * lookVector.x;

        camera.transform.localEulerAngles = Vector3.right * Mathf.Clamp(-lookVector.y, -90, 90);
    }




}
