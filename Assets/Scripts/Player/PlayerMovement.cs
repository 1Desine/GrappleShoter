using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    Player player;

    [Header("Ground")]
    [SerializeField] float walkMoveForce;
    [SerializeField] float walkStopForce;
    [SerializeField] float walkMaxSpeed;
    [SerializeField] float groundFrictionForce;
    [SerializeField] float jumpForce;

    [Header("Air")]
    [SerializeField] float airMoveForce;
    [SerializeField] float airStopForce;
    [SerializeField] float airMaxSpeed;

    Vector2 lookVector;



    private void Awake() {
        player = GetComponent<Player>();
    }
    private void Start() {
        player.OnUpdate += Player_OnPlayerUpdate;
    }


    void Player_OnPlayerUpdate() {
        if (player.isAlive) {
            Looking();
            Movement();
        }
        else {
            player.lookVerticalPivot.localEulerAngles = Vector3.zero;
        }

    }
    RaycastHit[] GetGroundCheckHits => Physics.SphereCastAll(transform.position + Vector3.up * 0.5f, 0.49f, Vector3.down, 0.1f);
    void Movement() {
        Vector2 moveInput = InputManager.Instance.GetMoveVector2();
        Vector3 desiredMoveDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        bool wannaSlowDown = Input.GetKey(KeyCode.LeftControl);


        RaycastHit[] groundHits = GetGroundCheckHits;
        if (groundHits.Length == 1) {
            // move while airborn

            // move forward
            Vector3 forwardForce = transform.forward;
            if (Vector3.Dot(forwardForce * moveInput.y, player.body.velocity) < 0)
                forwardForce *= airStopForce;
            else if (Vector3.Dot((forwardForce * moveInput.y).normalized, player.body.velocity) < airMaxSpeed)
                forwardForce *= airMoveForce;
            forwardForce *= moveInput.y;

            // move right
            Vector3 rightForce = transform.right;
            if (Vector3.Dot(rightForce * moveInput.x, player.body.velocity) < 0)
                rightForce *= airStopForce;
            else if (Vector3.Dot((rightForce * moveInput.x).normalized, player.body.velocity) < airMaxSpeed)
                rightForce *= airMoveForce;
            rightForce *= moveInput.x;

            // apply move force
            player.body.AddForce((forwardForce + rightForce) * Time.deltaTime);
        }
        else {
            foreach (RaycastHit hit in groundHits) {
                if (hit.collider.transform == transform) continue;

                // move forward
                Vector3 forwardByNormal = Vector3.Dot(transform.forward, Vector3.Cross(transform.right, hit.normal).normalized) * Vector3.Cross(transform.right, hit.normal).normalized;
                if (Vector3.Dot(forwardByNormal * moveInput.y, player.body.velocity) < 0)
                    forwardByNormal *= walkStopForce;
                else if (Vector3.Dot((forwardByNormal * moveInput.y).normalized, player.body.velocity) < walkMaxSpeed)
                    forwardByNormal *= walkMoveForce;
                forwardByNormal *= moveInput.y;

                // move right
                Vector3 rightByNormal = Vector3.Dot(transform.right, Vector3.Cross(transform.forward, hit.normal).normalized) * Vector3.Cross(transform.forward, hit.normal).normalized;
                if (Vector3.Dot(rightByNormal * moveInput.x, player.body.velocity) < 0)
                    rightByNormal *= walkStopForce;
                else if (Vector3.Dot((rightByNormal * moveInput.x).normalized, player.body.velocity) < walkMaxSpeed)
                    rightByNormal *= walkMoveForce;
                rightByNormal *= moveInput.x;

                // apply move force
                player.body.AddForce((forwardByNormal + rightByNormal) / (groundHits.Length - 1) * Time.deltaTime);


                // jump
                if (Input.GetKeyDown(KeyCode.Space)) Jump();

                // stop
                if (wannaSlowDown) player.body.velocity = Vector3.zero;

                // ground friction
                player.body.AddForce(Vector3.ClampMagnitude(-player.body.velocity, 1) * groundFrictionForce * Time.deltaTime);
            }
        }
    }
    void Jump() => player.body.AddForce(Vector3.up * jumpForce);
    void Looking() {
        Vector2 lookInput = InputManager.Instance.GetLookVector2Delta() * 0.1f;
        lookVector += lookInput;
        lookVector.y = Mathf.Clamp(lookVector.y, -90, 90);

        transform.eulerAngles = Vector3.up * lookVector.x;
        player.lookVerticalPivot.localEulerAngles = -Vector3.right * lookVector.y;
    }


    public void Push(Vector3 pushForce) => player.body.AddForce(pushForce);
    public void Push(Vector3 pushForce, Vector3 position) => player.body.AddForceAtPosition(pushForce, position);


}
