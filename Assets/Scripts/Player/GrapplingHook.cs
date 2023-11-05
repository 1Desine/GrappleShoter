using Unity.VisualScripting;
using Unity.VisualScripting.Dependencies.Sqlite;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class GrapplingHook : MonoBehaviour {

    [SerializeField] Player player;
    [SerializeField] GameObject visual;
    [SerializeField] LineRenderer ropeLine;
    [SerializeField] float ropeAnchorDistance;
    [SerializeField] float ropeStiffness;
    [SerializeField] float ropeDamper;
    [SerializeField] float pullRopeSpeed;

    [SerializeField] float swingForce;
    [SerializeField] float maxInputAngle;
    [SerializeField] float maxSwingVelocity;



    private void Awake() {
        player.OnStart += Player_OnStart;
        player.OnUpdate += Player_OnUpdate;
    }    
    void Player_OnStart() {
        visual.layer = 6;
        ropeLine.gameObject.SetActive(true);
    }
    void Player_OnUpdate() {
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
        float coefficientUp = Mathf.Clamp(Vector3.SignedAngle(player.lookVerticalPivot.forward, ropeDirecion, normalRight), -maxInputAngle, maxInputAngle) / maxInputAngle;
        float coefficientRight = Mathf.Clamp(Vector3.SignedAngle(player.lookVerticalPivot.forward, ropeDirecion, -normalUp), -maxInputAngle, maxInputAngle) / maxInputAngle;

        // Debug
        Debug.DrawRay(player.lookVerticalPivot.transform.position + ropeDirecion.normalized * 5, normalUp * coefficientUp, Color.red);
        Debug.DrawRay(player.lookVerticalPivot.transform.position + ropeDirecion.normalized * 5, normalRight * coefficientRight, Color.blue);

        // force up
        if (Vector3.Dot(player.body.velocity, (normalUp * coefficientUp).normalized) < maxSwingVelocity) player.body.AddForce(normalUp * coefficientUp * swingForce * Time.deltaTime);

        // force right
        if (Vector3.Dot(player.body.velocity, (normalRight * coefficientRight).normalized) < maxSwingVelocity) player.body.AddForce(normalRight * coefficientRight * swingForce * Time.deltaTime);
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
        if (player.rope == null) ropeLine.enabled = false;

        else {
            ropeLine.enabled = true;
            ropeLine.SetPosition(0, player.transform.TransformPoint(player.rope.anchor));
            if (player.rope.connectedBody != null) {
                ropeLine.SetPosition(1, player.rope.connectedBody.position);
            }
            else ropeLine.SetPosition(1, player.rope.connectedAnchor);
        }
    }
}
