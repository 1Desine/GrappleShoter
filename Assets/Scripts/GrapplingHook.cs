using System.Collections;
using UnityEngine;

public class GrapplingHook : MonoBehaviour {
    Player player;

    [SerializeField] Transform anchorTransform;

    float pullUpSpeed = 20;



    Vector3 anchorPosition;



    private void Awake() {
        player = GetComponent<Player>();
    }


    private void Update() {
        if (anchorPosition == Vector3.zero) anchorTransform.localPosition = Vector3.zero;
        else anchorTransform.position = anchorPosition;


        if (Input.GetKey(KeyCode.LeftShift) && anchorPosition == Vector3.zero) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Physics.Raycast(player.camera.transform.position, player.camera.transform.forward, out RaycastHit hit);

            SetAnchorPosition(hit.point);
        }
        if (Input.GetKeyUp(KeyCode.LeftShift)) {
            ResetAnchorPosition();
        }

        if (Input.GetKeyDown(KeyCode.Space)) {
            PullUp();
        }
    }

    public void SetAnchorPosition(Vector3 anchorPosition) {
        if (anchorPosition == Vector3.zero) return;

        this.anchorPosition = anchorPosition;
        anchorTransform.position = anchorPosition;

        player.springJoint.maxDistance = (anchorPosition - transform.position).magnitude;
        player.springJoint.spring = 1000;
    }
    public void ResetAnchorPosition() {
        anchorPosition = Vector3.zero;
        player.springJoint.spring = 0;
        player.springJoint.maxDistance = 0;
    }


    public void PullUp() {
        if (anchorPosition != Vector3.zero) player.springJoint.spring = 50;
        StartCoroutine(PullRopeCoroutine());
    }

    IEnumerator PullRopeCoroutine() {
        if (player.springJoint.maxDistance > (anchorPosition - transform.position).magnitude) player.springJoint.maxDistance = (anchorPosition - transform.position).magnitude;

        player.springJoint.maxDistance -= pullUpSpeed * Time.deltaTime;
        yield return new WaitForSeconds(Time.deltaTime);
        if (player.springJoint.maxDistance > 0 && anchorPosition != Vector3.zero) StartCoroutine(PullRopeCoroutine());
    }


}
