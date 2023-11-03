using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    Player player;

    [SerializeField] float moveForce;
    [SerializeField] AnimationCurve moveForceCurve;
    [SerializeField] float maxSpeedByWalk;
    [SerializeField] float walkStoppingForce;
    [SerializeField] float airMoveForce;
    [SerializeField] float airbornStoppingForce;
    [SerializeField] float groundFrictionForce;
    [SerializeField] float jumpBuffering;
    float lastJumpRequestTime;
    bool wannaJump;

    Vector2 lookVector;



    private void Awake() {
        player = GetComponent<Player>();
    }
    private void Start() {
        player.OnPlayerUpdate += Player_OnPlayerUpdate;
    }


    void Player_OnPlayerUpdate() {
        if (player.isAlive) {
            Looking();
            Movement();
        }
        else {
            player.lookVerticalPivot.localEulerAngles = Vector3.zero;
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            lastJumpRequestTime = Time.time;
            wannaJump = true;
        }
        if (Input.GetKeyUp(KeyCode.Space)) wannaJump = false;
    }
    RaycastHit[] GetGroundCheckHits => Physics.SphereCastAll(transform.position + Vector3.up * 0.5f, 0.49f, Vector3.down, 0.1f);
    void Movement() {
        Vector2 moveInput = GameInput.Instance.GetMoveVector2();
        Vector3 desiredMoveDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        bool wannaSlowDown = Input.GetKey(KeyCode.LeftControl);


        RaycastHit[] groundHits = GetGroundCheckHits;
        if (groundHits.Length == 1) {
            // move while airborn

            // move forward
            float forwardForce = 0;
            if (Vector3.Dot(transform.forward * moveInput.y, player.body.velocity) > 0)
                forwardForce = airMoveForce;
            else forwardForce = airbornStoppingForce;
            player.body.AddForce(transform.forward * moveInput.y * forwardForce * Time.deltaTime);

            // move right
            float rightFroce = 0;
            if (Vector3.Dot(transform.right * moveInput.x, player.body.velocity) > 0)
                rightFroce = airMoveForce;
            else rightFroce = airbornStoppingForce;
            player.body.AddForce(transform.right * moveInput.x * rightFroce * Time.deltaTime);
        }
        else {
            foreach (RaycastHit hit in groundHits) {
                if (hit.collider.transform == transform) continue;
                /* Works but weird 
                                    
                // move
                var forwardByNormal = Vector3.Dot(transform.forward, Vector3.Cross(transform.right, hit.normal).normalized) * Vector3.Cross(transform.right, hit.normal).normalized;
                var rightByNormal = Vector3.Dot(transform.right, Vector3.Cross(transform.forward, hit.normal).normalized) * Vector3.Cross(transform.forward, hit.normal).normalized;
                Vector3 desiredMoveByNormad = forwardByNormal * moveInput.y + rightByNormal * moveInput.x;

                float forceToApply = 0;
                if (Vector3.Dot(desiredMoveByNormad, player.body.velocity) > 0)
                    forceToApply = moveForce * moveForceCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(Vector3.Dot(desiredMoveByNormad, player.body.velocity)) / maxSpeedByWalk));
                else forceToApply = stopForce;
                player.body.AddForce(desiredMoveByNormad * forceToApply / (sphereHits.Length - 1) * Time.deltaTime);
                */


                // move forward
                var forwardByNormal = Vector3.Dot(transform.forward, Vector3.Cross(transform.right, hit.normal).normalized) * Vector3.Cross(transform.right, hit.normal).normalized;
                float forwardForce = 0;
                if (Vector3.Dot(forwardByNormal * moveInput.y, player.body.velocity) > 0)
                    forwardForce = moveForce * moveForceCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(Vector3.Dot(forwardByNormal * moveInput.y, player.body.velocity)) / maxSpeedByWalk));
                else forwardForce = walkStoppingForce;
                player.body.AddForce(forwardByNormal * moveInput.y * forwardForce / (groundHits.Length - 1) * Time.deltaTime);

                // move right
                var rightByNormal = Vector3.Dot(transform.right, Vector3.Cross(transform.forward, hit.normal).normalized) * Vector3.Cross(transform.forward, hit.normal).normalized;
                float rightFroce = 0;
                if (Vector3.Dot(rightByNormal * moveInput.x, player.body.velocity) > 0)
                    rightFroce = moveForce * moveForceCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(Vector3.Dot(rightByNormal * moveInput.x, player.body.velocity)) / maxSpeedByWalk));
                else rightFroce = walkStoppingForce;
                player.body.AddForce(rightByNormal * moveInput.x * rightFroce / (groundHits.Length - 1) * Time.deltaTime);


                // jump
                if (Time.time - lastJumpRequestTime < jumpBuffering
                    && wannaJump) {
                    Jump();
                    wannaJump = false;
                }

                // stop
                if (wannaSlowDown) player.body.velocity = Vector3.zero;

                // ground friction
                player.body.AddForce(Vector3.ClampMagnitude(-player.body.velocity, 1) * groundFrictionForce * Time.deltaTime);
            }
        }
    }
    void Looking() {
        Vector2 lookInput = GameInput.Instance.GetLookVector2Delta() * player.playerSettings.sensitivity;
        lookVector += lookInput;

        transform.eulerAngles = Vector3.up * lookVector.x;

        player.lookVerticalPivot.localEulerAngles = Vector3.right * Mathf.Clamp(-lookVector.y, -90, 90);
    }
    void Jump() => player.body.AddForce(Vector3.up * 500);


    public void Push(Vector3 pushForce) => player.body.AddForce(pushForce);
    public void Push(Vector3 pushForce, Vector3 position) => player.body.AddForceAtPosition(pushForce, position);


}
