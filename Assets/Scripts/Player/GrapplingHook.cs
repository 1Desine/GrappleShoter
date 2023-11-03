using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : MonoBehaviour {
    Player player;

    [SerializeField] GameObject visual;
    [SerializeField] float ropeAnchorDistance;
    [SerializeField] float ropeStiffness;
    [SerializeField] float ropeDamper;
    [SerializeField] float pullRopeSpeed;

    [SerializeField] float swingForce;
    [SerializeField] float maxSwingForce;
    [SerializeField] float maxSwingVelocity;

    LineRenderer lineRenderer;


    private void Awake() {
        player = transform.parent.GetComponent<Player>();
        lineRenderer = GetComponent<LineRenderer>();
        visual.layer = 6;
    }
    private void Start() {
        player.OnUpdate += Player_OnPlayerUpdate;
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
        if (Input.GetKey(KeyCode.Space) == false) return;

        if (player.rope.maxDistance > (player.rope.connectedAnchor - transform.position).magnitude)
            player.rope.maxDistance = (player.rope.connectedAnchor - transform.position).magnitude;
        player.rope.maxDistance -= pullRopeSpeed * Time.deltaTime;

        // offset force 
        Vector3 ropeDirecion = (player.rope.connectedAnchor - transform.position).normalized;
        Vector3 normalRight = Vector3.Cross(Vector3.up, ropeDirecion).normalized;
        Vector3 normalUp = Vector3.Cross(ropeDirecion, normalRight).normalized;
        Vector3 coefficientUp = normalUp * Vector3.Dot(player.lookVerticalPivot.forward, normalUp);
        Vector3 coefficientRight = normalRight * Vector3.Dot(player.lookVerticalPivot.forward, normalRight);


        // Debug
        Debug.DrawRay(player.lookVerticalPivot.transform.position + ropeDirecion.normalized * 5, Vector3.ClampMagnitude(coefficientUp * swingForce, maxSwingForce) / maxSwingForce, Color.red);
        Debug.DrawRay(player.lookVerticalPivot.transform.position + ropeDirecion.normalized * 5, Vector3.ClampMagnitude(coefficientRight * swingForce, maxSwingForce) / maxSwingForce, Color.blue);

        // force up
        if (Vector3.Dot(player.body.velocity, coefficientUp.normalized) < maxSwingVelocity)
            player.body.AddForce(Vector3.ClampMagnitude(coefficientUp * swingForce, maxSwingForce) * Time.deltaTime);

        // force right
        if (Vector3.Dot(player.body.velocity, coefficientRight.normalized) < maxSwingVelocity)
            player.body.AddForce(Vector3.ClampMagnitude(coefficientRight * swingForce, maxSwingForce) * Time.deltaTime);
    }
    void UpdateRopeAnchor() {
        if (player.rope.connectedBody == null) {
            float dotForward = Vector3.Dot(player.rope.connectedAnchor - transform.position, transform.forward);
            float dotRight = Vector3.Dot(player.rope.connectedAnchor - transform.position, transform.right);
            player.rope.anchor = transform.localPosition + new Vector3(dotRight, 0, dotForward).normalized * ropeAnchorDistance;
        }
        else {
            float dotForward = Vector3.Dot(player.rope.connectedBody.position - transform.position, transform.forward);
            float dotRight = Vector3.Dot(player.rope.connectedBody.position - transform.position, transform.right);
            player.rope.anchor = transform.localPosition + new Vector3(dotRight, 0, dotForward).normalized * ropeAnchorDistance;
        }
    }
    void HandleLineRenderer() {
        if (player.rope == null) lineRenderer.enabled = false;

        else {
            lineRenderer.enabled = true;
            lineRenderer.SetPosition(0, player.transform.TransformPoint(player.rope.anchor));
            if (player.rope.connectedBody != null) {
                lineRenderer.SetPosition(1, player.rope.connectedBody.position);
            }
            else lineRenderer.SetPosition(1, player.rope.connectedAnchor);
        }
    }
}
