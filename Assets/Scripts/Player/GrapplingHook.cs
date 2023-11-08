using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : MonoBehaviour {

    [SerializeField] PlayerObject player;
    [SerializeField] GameObject visual;
    [SerializeField] LineRenderer ropeLine;
    [SerializeField] float ropeAnchorDistance;
    [Header("Settings")]
    [SerializeField] float minRopeLenght;
    [SerializeField] float maxRopeConnectDistance;
    [SerializeField] float ropeStiffness;
    [SerializeField] float ropeDamper;
    [SerializeField] float maxInputAngle;
    [Header("Movement")]
    [SerializeField] float pullRopeSpeed;
    [SerializeField] float swingForceUp;
    [SerializeField] float swingForceRight;
    [SerializeField] float maxSwingVelocityUp;
    [SerializeField] float maxSwingVelocityRight;



    private void Awake() {
        player.OnStart += Player_OnStart;
        player.OnUpdate += Player_OnUpdate;
    }
    void Player_OnStart() {
        visual.layer = 6;
        ropeLine.gameObject.SetActive(true);
    }
    void Player_OnUpdate() {
        if (player.isAlive == false) return;

        if (Input.GetKey(KeyCode.LeftShift) && player.rope == null) {
            if (Physics.Raycast(player.lookVerticalPivot.position, player.lookVerticalPivot.forward, out RaycastHit hit, maxRopeConnectDistance)) {
                if (hit.collider.TryGetComponent(out Rigidbody body)) AttachRope(body);
                else AttachRope(hit.point);
            }
        }
        if (Input.GetKeyUp(KeyCode.LeftShift)) {
            DetachRope();
        }
    }
    private void FixedUpdate() {
        HandleRopePulling();

        UpdateRopeAnchor();
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
        if (player.rope == null
            || player.isAlive == false
            || player.rope.maxDistance < minRopeLenght) return;

        if (player.rope.maxDistance > (player.rope.connectedAnchor - transform.position).magnitude)
            player.rope.maxDistance = (player.rope.connectedAnchor - transform.position).magnitude;
        float pullRopeSpeedToApply = Mathf.Lerp(1, pullRopeSpeed, (player.rope.maxDistance - minRopeLenght) / maxRopeConnectDistance);
        player.rope.maxDistance -= pullRopeSpeed * Time.fixedDeltaTime;
        //Debug.Log(pullRopeSpeedToApply);

        Vector2 moveInput = InputManager.Instance.GetMoveVector2();
        Vector3 ropeDirecion = (player.rope.connectedAnchor - transform.position).normalized;
        Vector3 normalRight = Vector3.Cross(Vector3.up, ropeDirecion).normalized;
        Vector3 normalUp = Vector3.Cross(ropeDirecion, normalRight).normalized;
        normalUp *= moveInput.y;
        normalRight *= moveInput.x;

        // force up
        if (Vector3.Dot(player.body.velocity, normalUp) < maxSwingVelocityUp) player.body.AddForce(normalUp * swingForceUp * Time.fixedDeltaTime);

        // force right
        if (Vector3.Dot(player.body.velocity, normalRight) < maxSwingVelocityRight) player.body.AddForce(normalRight * swingForceRight * Time.fixedDeltaTime);
    }
    void UpdateRopeAnchor() {
        if (player.rope == null) return;

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
        if (player.rope == null) {
            ropeLine.enabled = false;
            return;
        }
        ropeLine.enabled = true;

        ropeLine.SetPosition(0, player.transform.TransformPoint(player.rope.anchor));
        if (player.rope.connectedBody != null) {
            ropeLine.SetPosition(1, player.rope.connectedBody.position);
        }
        else ropeLine.SetPosition(1, player.rope.connectedAnchor);
    }
}
