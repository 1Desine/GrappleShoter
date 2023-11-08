using Unity.Netcode;
using Unity.Netcode.Components;
using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : NetworkBehaviour {

    [SerializeField] PlayerObject player;
    [SerializeField] GameObject visual;
    [SerializeField] LineRenderer ropeLine;

    NetworkVariable<Vector3> anchorPointNV = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );
    NetworkVariable<Vector3> connectedAnchorPointNV = new NetworkVariable<Vector3>(
        Vector3.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
        );

    [Header("Settings")]
    [SerializeField] float ropeAnchorDistance;
    [SerializeField] float coolDown;
    [Header("SpringJoint")]
    [SerializeField] float minRopeLenght;
    [SerializeField] float maxRopeLenght;
    [SerializeField] float ropeStiffness;
    [SerializeField] float ropeDamper;
    [Header("Swinging")]
    [SerializeField] float pullRopeSpeed;
    [SerializeField] float swingForceUp;
    [SerializeField] float swingForceRight;
    [SerializeField] float maxSwingVelocityUp;
    [SerializeField] float maxSwingVelocityRight;

    float lastConnectedHookTime;

    private void Awake() {
        player.OnStart += Player_OnStart;
        player.OnUpdate += Player_OnUpdate;

        anchorPointNV.OnValueChanged += (_, _) => { HandleLineRenderer(); };
        connectedAnchorPointNV.OnValueChanged += (_, _) => { HandleLineRenderer(); };
    }
    void Player_OnStart() {
        visual.layer = 6;
    }
    void Player_OnUpdate() {
        if (player.isAlive) {
            if (Input.GetKey(KeyCode.LeftShift)
                && player.rope == null
                && Time.time - lastConnectedHookTime > coolDown) {
                if (Physics.Raycast(player.lookVerticalPivot.position, player.lookVerticalPivot.forward, out RaycastHit hit, maxRopeLenght)) {
                    if (hit.collider.TryGetComponent(out Rigidbody body)) AttachRope(body); 
                    else AttachRope(hit.point);
                }
            }
            if (Input.GetKeyUp(KeyCode.LeftShift)) {
                DetachRope();
            }
        }

        UpdateRopeAnchor();
    }
    private void FixedUpdate() {
        HandleRopePulling();
    }

    void AttachRope(Vector3 anchorPosition) {
        player.rope = player.AddComponent<SpringJoint>();
        player.rope.autoConfigureConnectedAnchor = false;
        player.rope.connectedAnchor = anchorPosition;

        player.rope.maxDistance = Mathf.Max((anchorPosition - transform.position).magnitude, minRopeLenght);
        player.rope.spring = ropeStiffness;
        player.rope.damper = ropeDamper;

        lastConnectedHookTime = Time.time;
    }
    void AttachRope(Rigidbody body) {
        player.rope = player.AddComponent<SpringJoint>();
        player.rope.autoConfigureConnectedAnchor = false;
        player.rope.connectedBody = body;

        player.rope.maxDistance = Mathf.Max((body.position - transform.position).magnitude, minRopeLenght);
        player.rope.spring = ropeStiffness;
        player.rope.damper = ropeDamper;

        lastConnectedHookTime = Time.time;
    }
    void DetachRope() {
        Destroy(player.rope);
    }
    void HandleRopePulling() {
        if (player.rope == null
            || player.isAlive == false) return;

        if (player.rope.maxDistance > minRopeLenght) {
            if (player.rope.maxDistance > (player.rope.connectedAnchor - transform.position).magnitude)
                player.rope.maxDistance = (player.rope.connectedAnchor - transform.position).magnitude;
            float pullRopeSpeedToApply = Mathf.Lerp(pullRopeSpeed / 3, pullRopeSpeed, (player.rope.maxDistance - minRopeLenght) / maxRopeLenght);
            player.rope.maxDistance -= pullRopeSpeedToApply * Time.fixedDeltaTime;
            //Debug.Log(pullRopeSpeedToApply);
        }

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
        if (player.rope != null) {
            if (player.rope.connectedBody == null) {
                float dotForward = Vector3.Dot(player.rope.connectedAnchor - transform.position, transform.forward);
                float dotRight = Vector3.Dot(player.rope.connectedAnchor - transform.position, transform.right);
                player.rope.anchor = transform.localPosition + new Vector3(dotRight, 0, dotForward).normalized * ropeAnchorDistance;
                connectedAnchorPointNV.Value = player.rope.connectedAnchor;
            }
            else {
                float dotForward = Vector3.Dot(player.rope.connectedBody.position - transform.position, transform.forward);
                float dotRight = Vector3.Dot(player.rope.connectedBody.position - transform.position, transform.right);
                player.rope.anchor = transform.localPosition + new Vector3(dotRight, 0, dotForward).normalized * ropeAnchorDistance;
                connectedAnchorPointNV.Value = player.rope.connectedBody.position;
            }

            anchorPointNV.Value = player.transform.TransformPoint(player.rope.anchor);
        }
        else {
            anchorPointNV.Value = Vector3.zero;
            connectedAnchorPointNV.Value = Vector3.zero;
        }
    }
    void HandleLineRenderer() {
        if (anchorPointNV.Value == Vector3.zero 
            && connectedAnchorPointNV.Value == Vector3.zero) {
            ropeLine.enabled = false;
            return;
        }
        ropeLine.enabled = true;

        ropeLine.SetPosition(0, anchorPointNV.Value);
        ropeLine.SetPosition(1, connectedAnchorPointNV.Value);
    }
}
