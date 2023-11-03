using UnityEngine;

public class GrapplingHook : MonoBehaviour {
    Player player;

    [SerializeField] float springStiffness;
    [SerializeField] float pullRopeSpeed;
    [SerializeField] float ropeLength;

    Connection connection;
    enum Connection {
        None,
        Static,
        Rigidbody,
    }
    Vector3 connectionPoint;
    Rigidbody connectionRigidbody;

    bool wannaPullRope;


    private void Awake() {
        player = transform.parent.GetComponent<Player>();
    }
    private void Start() {
        player.OnPlayerUpdate += Player_OnPlayerUpdate;
    }

    void Player_OnPlayerUpdate() {
        ApplyRopeForce();


        if (player.isAlive) {
            if (Input.GetKey(KeyCode.LeftShift) && connection == Connection.None) {
                if (Physics.Raycast(player.lookVerticalPivot.position, player.lookVerticalPivot.forward, out RaycastHit hit)) {
                    if (hit.collider.TryGetComponent(out Rigidbody body)) AttachRope(body);
                    else AttachRope(hit.point);
                }
            }
            if (Input.GetKeyUp(KeyCode.LeftShift)) {
                DetachRope();
            }
            wannaPullRope = Input.GetKey(KeyCode.Space);
        }

        HandleRopePulling();
    }

    void AttachRope(Vector3 anchorPosition) {
        connection = Connection.Static;
        connectionPoint = anchorPosition;
        ropeLength = (anchorPosition - transform.position).magnitude;
    }
    void AttachRope(Rigidbody body) {
        connection = Connection.Rigidbody;
        connectionRigidbody = body;
        ropeLength = (body.position - transform.position).magnitude;
    }
    void DetachRope() {
        connection = Connection.None;
        connectionPoint = Vector3.zero;
        connectionRigidbody = null;
    }
    Vector3 GetConnectionPoint() {
        if (connection == Connection.Static) return connectionPoint;
        if (connection == Connection.Rigidbody
            && connectionRigidbody != null)
            return connectionRigidbody.position;

        Debug.LogError("me. no connection with rope");
        return Vector3.zero;
    }
    void ApplyRopeForce() {
        if (connection == Connection.None) return;

        Debug.DrawLine(transform.position, connectionPoint, Color.black);

        Vector3 pullDirection = GetConnectionPoint() - transform.position;
        if (pullDirection.magnitude < ropeLength) return;

        Vector3 pullVector = pullDirection * (pullDirection.magnitude - ropeLength) * springStiffness;

        player.body.AddForceAtPosition(pullVector * Time.deltaTime, transform.position);

        if (connection == Connection.Rigidbody) {
            connectionRigidbody.AddForce(-pullVector * Time.deltaTime);
        }
    }

    void HandleRopePulling() {
        if (wannaPullRope == false
            || connection == Connection.None) return;

        if (ropeLength > (GetConnectionPoint() - transform.position).magnitude)
            ropeLength = (GetConnectionPoint() - transform.position).magnitude;

        ropeLength = Mathf.Max(ropeLength - pullRopeSpeed * Time.deltaTime, 0);
    }

}
