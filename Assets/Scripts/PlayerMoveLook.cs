using UnityEngine;

public class PlayerMoveLook : MonoBehaviour {
    Player player;

    [SerializeField] float moveForce;
    [SerializeField] AnimationCurve moveForceCurve;
    [SerializeField] float stopForce;
    [SerializeField] float airMoveForce;
    [SerializeField] float maxSpeedByWalk;
    [SerializeField] float jumpBuffering;
    float lastJumpRequestTime;
    bool wannaJump;

    Vector2 lookVector;



    private void Awake() {
        player = GetComponent<Player>();
    }


    private void Update() {
        if (player.isAlive) {
            Looking();
            Movement();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            lastJumpRequestTime = Time.time;
            wannaJump = true;
        }
        if (Input.GetKeyUp(KeyCode.Space)) wannaJump = false;

    }

    void Movement() {
        Vector2 moveInput = GameInput.Instance.GetMoveVector2();
        Vector3 desiredMoveDir = transform.forward * moveInput.y + transform.right * moveInput.x;

        RaycastHit[] sphereHits = Physics.SphereCastAll(transform.position + Vector3.up * 0.5f, 0.5f, Vector3.down, 0.1f);
        if (sphereHits.Length == 1) {
            // move while airborn
            player.body.AddForce(desiredMoveDir * airMoveForce * Time.deltaTime);
        }
        else {
            foreach (RaycastHit hit in sphereHits) {
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
                else forwardForce = stopForce;
                player.body.AddForce(forwardByNormal * moveInput.y * forwardForce / (sphereHits.Length - 1) * Time.deltaTime);

                // move right
                var rightByNormal = Vector3.Dot(transform.right, Vector3.Cross(transform.forward, hit.normal).normalized) * Vector3.Cross(transform.forward, hit.normal).normalized;
                float rightFroce = 0;
                if (Vector3.Dot(rightByNormal * moveInput.x, player.body.velocity) > 0)
                    rightFroce = moveForce * moveForceCurve.Evaluate(Mathf.Clamp01(Mathf.Abs(Vector3.Dot(rightByNormal * moveInput.x, player.body.velocity)) / maxSpeedByWalk));
                else rightFroce = stopForce;
                player.body.AddForce(rightByNormal * moveInput.x * rightFroce / (sphereHits.Length - 1) * Time.deltaTime);


                // jump
                if (Time.time - lastJumpRequestTime < jumpBuffering 
                    && wannaJump) {
                    Jump();
                    wannaJump = false;
                }

                // stop
                if (Input.GetKey(KeyCode.LeftControl)) player.body.velocity = Vector3.zero;
            }
        }
    }
    void Looking() {
        Vector2 lookInput = GameInput.Instance.GetLookVector2Delta() * player.playerSettings.sensitivity;
        lookVector += lookInput;

        transform.eulerAngles = Vector3.up * lookVector.x;

        player.camera.transform.localEulerAngles = Vector3.right * Mathf.Clamp(-lookVector.y, -90, 90);
    }
    void Jump() => player.body.AddForce(Vector3.up * 500);


}
