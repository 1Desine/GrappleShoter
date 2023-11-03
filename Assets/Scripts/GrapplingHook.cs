using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : MonoBehaviour {
    Player player;

    [SerializeField] float ropeStiffness;
    [SerializeField] float ropeDamper;
    [SerializeField] float pullRopeSpeed;
    [SerializeField] float ropePivotDistance;

    LineRenderer lineRenderer;

    bool wannaPullRope;


    private void Awake() {
        player = transform.parent.GetComponent<Player>();
        lineRenderer = GetComponent<LineRenderer>();
    }
    private void Start() {
        player.OnPlayerUpdate += Player_OnPlayerUpdate;
    }

    void Player_OnPlayerUpdate() {
        if (player.isAlive) {
            if (Input.GetKey(KeyCode.LeftShift) && player.rope == null) {
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

        if (player.rope != null) {
            HandleRopePulling();
            UpdateRopeAnchor();
        }

        HandleLineRenderer();
    }

    void AttachRope(Vector3 anchorPosition) {
        player.rope = player.AddComponent<SpringJoint>();
        player.rope.autoConfigureConnectedAnchor = false;
        player.rope.connectedAnchor = anchorPosition;

        player.rope.maxDistance = (anchorPosition - transform.position).magnitude;
        player.rope.spring = ropeStiffness;
        player.rope.damper = ropeDamper;
    }
    void AttachRope(Rigidbody body) {
        player.rope = player.AddComponent<SpringJoint>();
        player.rope.autoConfigureConnectedAnchor = false;
        player.rope.connectedBody = body;

        player.rope.maxDistance = (body.position - transform.position).magnitude;
        player.rope.spring = ropeStiffness;
        player.rope.damper = ropeDamper;
    }
    void DetachRope() => Destroy(player.rope);
    void HandleRopePulling() {
        if (wannaPullRope == false) return;

        if (player.rope.maxDistance > (player.rope.connectedAnchor - transform.position).magnitude)
            player.rope.maxDistance = (player.rope.connectedAnchor - transform.position).magnitude;

        player.rope.maxDistance -= pullRopeSpeed * Time.deltaTime;
    }
    void UpdateRopeAnchor() {
        float dotForward = Vector3.Dot(player.rope.connectedAnchor - transform.position, transform.forward);
        float dotRight = Vector3.Dot(player.rope.connectedAnchor - transform.position, transform.right);
        player.rope.anchor = transform.localPosition + new Vector3(dotRight, 0, dotForward).normalized * ropePivotDistance;
    }
    void HandleLineRenderer() {
        if (player.rope == null) lineRenderer.enabled = false;

        else {
            lineRenderer.enabled = true;
            lineRenderer.SetPositions(new Vector3[] { player.transform.TransformPoint(player.rope.anchor), player.rope.connectedAnchor });
        }
    }
}
