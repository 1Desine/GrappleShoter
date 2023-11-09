using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class GrapplingHook : NetworkBehaviour {

    [SerializeField] PlayerObject player;
    [SerializeField] GameObject visual;
    [SerializeField] LineRenderer ropeLine;

    NetworkVariable<bool> ropeConnectedNV = new NetworkVariable<bool>(
        false,
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
        player.OnFixedUpdate += Player_OnFixedUpdate;


        connectedAnchorPointNV.OnValueChanged += (p, c) => {
            ropeLine.SetPosition(1, c);
        };
        ropeConnectedNV.OnValueChanged += (p, c) => {
            ropeLine.enabled = c;
        };
    }
    void Update() {
        if (ropeConnectedNV.Value) {
            float dotForward = Vector3.Dot(connectedAnchorPointNV.Value - transform.position, transform.forward);
            float dotRight = Vector3.Dot(connectedAnchorPointNV.Value - transform.position, transform.right);
            Vector3 anchor = transform.localPosition + new Vector3(dotRight, 0, dotForward).normalized * ropeAnchorDistance;

            ropeLine.SetPosition(0, player.transform.TransformPoint(anchor));
        }
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

        UpdateRopeAnchors();
    }
    void Player_OnFixedUpdate() {
        HandleRopePulling();
    }

    void AttachRope(object anchor) {
        player.rope = player.AddComponent<SpringJoint>();
        player.rope.autoConfigureConnectedAnchor = false;
        player.rope.spring = ropeStiffness;
        player.rope.damper = ropeDamper;

        if (anchor is Vector3 vecotor) {
            player.rope.connectedAnchor = vecotor;
        }
        else if (anchor is Rigidbody body) {
            player.rope.connectedAnchor = body.position;
        }
        player.rope.maxDistance = Mathf.Max((player.rope.connectedAnchor - transform.position).magnitude, minRopeLenght);

        connectedAnchorPointNV.Value = player.rope.connectedAnchor;

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
    void UpdateRopeAnchors() {
        if (ropeConnectedNV.Value = player.rope != null) {
            if (player.rope.connectedBody != null) {
                connectedAnchorPointNV.Value = player.rope.connectedBody.position;
            }
        }
    }
}
