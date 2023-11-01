using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class Player : NetworkBehaviour {


    public Transform lookVerticalPivot;
    public new Camera camera;
    public Rigidbody body;
    public SpringJoint springJoint;
    public PlayerSettings playerSettings;
    public PlayerMovement playerMovement;
    public GunsHandler gunsHandler;

    public NetworkObject networkObject;

    int health = 100;
    public bool isAlive { get { return health > 0; } }


    public Action OnPlayerUpdate = () => { };


    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start() {
        GameManager.Instance.RegisterPlayer(this);

        if (IsOwner == false) camera.gameObject.SetActive(false);
    }

    private void Update() {
        if (IsOwner == false) return;

        OnPlayerUpdate();

        body.freezeRotation = isAlive;
    }



    [ServerRpc(RequireOwnership = false)]
    public void Damage_ServerRpc(int damage, Vector3 hitPoint, Vector3 pushVector, ulong playerId) {
        Damage_ClientRpc(damage, hitPoint, pushVector, new ClientRpcParams { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { playerId } } });
    }
    [ClientRpc]
    public void Damage_ClientRpc(int damage, Vector3 hitPoint, Vector3 pushVector, ClientRpcParams clientRpcParams) {
        if (health - damage <= 0 && isAlive) Die();
        health -= damage;

        playerMovement.Push(pushVector, hitPoint);
    }

    private void Die() {
        Debug.Log("me dead bro", this);
        StartCoroutine(SpawnCoroutine());
    }


    void Hide() {

    }
    void Show() {

    }

    IEnumerator SpawnCoroutine() {
        float randomFloat = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
        Vector3 randomV3 = new Vector3(Mathf.Cos(randomFloat), 0, Mathf.Sin(randomFloat));
        body.AddForceAtPosition(transform.position, randomV3 * 100);

        yield return new WaitForSeconds(3);
        Hide();
        yield return new WaitForSeconds(1);
        Spawn();
    }
    public void Spawn() {
        transform.position = GameManager.Instance.GetRandomSpawnPoint();
        health = 100;
        body.velocity = Vector3.zero;
        gunsHandler.ResetWeapons();
        Show();
    }

}
