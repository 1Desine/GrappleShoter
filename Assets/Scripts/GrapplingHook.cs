using System.Collections;
using UnityEngine;

public class GrapplingHook : MonoBehaviour {

    [SerializeField] Transform anchorTransform;

    float pullUpSpeed = 20;

    SpringJoint springJoint;


    Vector3 anchorPosition;


    private void Awake() {
        springJoint = GetComponent<SpringJoint>();
    }


    private void Update() {
        if (anchorPosition == Vector3.zero) anchorTransform.localPosition = Vector3.zero;
        else anchorTransform.position = anchorPosition;
    }

    public void SetAnchorPosition(Vector3 anchorPosition) {
        this.anchorPosition = anchorPosition;
        anchorTransform.position = anchorPosition;

        springJoint.maxDistance = (anchorPosition - transform.position).magnitude;
        springJoint.spring = 1000;
    }
    public void ResetAnchorPosition() {
        anchorPosition = Vector3.zero;
        springJoint.spring = 0;
    }


    public void PullUp() {
        if (anchorPosition != Vector3.zero) springJoint.spring = 50;
        StartCoroutine(PullRopeCoroutine());
    }

    IEnumerator PullRopeCoroutine() {
        springJoint.maxDistance -= pullUpSpeed * Time.deltaTime;
        yield return new WaitForSeconds(Time.deltaTime);
        if (springJoint.maxDistance > 0 && anchorPosition != Vector3.zero) StartCoroutine(PullRopeCoroutine());
    }


}
