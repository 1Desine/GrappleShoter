using System.Diagnostics;
using Unity.Netcode;
using UnityEngine;

public class BulletTrace : NetworkBehaviour {
    public Transform bullet;

    public Vector3 direction;

      
    private void Update() {
        bullet.transform.localPosition += direction * 100 * Time.deltaTime;
    }

}
