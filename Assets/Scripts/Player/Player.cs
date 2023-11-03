using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Player : NetworkBehaviour {


    public Transform lookVerticalPivot;
    public new Camera camera;
    public Rigidbody body;
    public PlayerMovement playerMovement;
    public GunsHandler gunsHandler;
    public SpringJoint rope { get; set; }

    public NetworkObject networkObject;

    int health = 100;
    public bool isAlive => health > 0;

    [SerializeField] float suffocationSpeed;
    [SerializeField] float desuffocationSpeed;
    float suffocation;


    public Action OnUpdate = () => { };


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

        OnUpdate();

        if (Input.GetKeyDown(KeyCode.LeftAlt)) Die();
        if (Input.GetKeyDown(KeyCode.LeftAlt)) health = 0;

        UpdateSuffocation();
    }

    void UpdateSuffocation() {
        float suffocationApply = 0;
        if (rope != null) {
            if (rope.currentForce.magnitude > 2) suffocationApply = Mathf.Min(rope.currentForce.magnitude, 5) * suffocationSpeed;
            else suffocationApply = -desuffocationSpeed;
        }
        else suffocationApply = -desuffocationSpeed;

        suffocation = Mathf.Clamp01(suffocation + suffocationApply * Time.deltaTime);

        if (PlayerUI.Instance != null) {
            PlayerUI.Instance.SetSuffocation(suffocation);
        }
    }


    [ServerRpc(RequireOwnership = false)]
    public void Damage_ServerRpc(int damage, Vector3 hitPoint, Vector3 pushVector, ulong playerId) {
        Damage_ClientRpc(damage, hitPoint, pushVector, playerId);
    }
    [ClientRpc]
    public void Damage_ClientRpc(int damage, Vector3 hitPoint, Vector3 pushVector, ulong playerId) {
        if (OwnerClientId != playerId) return;

        if (health - damage <= 0 && isAlive) Die();
        health -= damage;

        playerMovement.Push(pushVector, hitPoint);
    }

    private void Die() {
        Debug.Log("me dead bro", this);
        body.freezeRotation = false;
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

        yield return new WaitForSeconds(5);
        Hide();
        yield return new WaitForSeconds(1);
        Spawn();
    }
    public void Spawn() {
        body.freezeRotation = true;
        transform.position = GameManager.Instance.GetRandomSpawnPoint();
        health = 100;
        body.velocity = Vector3.zero;
        gunsHandler.ResetWeapons();
        Show();
    }

}
