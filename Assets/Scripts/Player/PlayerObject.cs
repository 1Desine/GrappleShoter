using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerObject : NetworkBehaviour {


    bool despawnInstructionHasBeenSent;
    public async void Die() {
        if (despawnInstructionHasBeenSent) return;
        despawnInstructionHasBeenSent = true;

        body.freezeRotation = false;
        body.constraints = RigidbodyConstraints.FreezeRotationY;
        float randomFloat = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
        Vector3 randomV3 = new Vector3(Mathf.Cos(randomFloat), 0, Mathf.Sin(randomFloat));
        body.AddForceAtPosition(transform.position, randomV3);

        await Task.Delay((int)(GameManager.Instance.timeToDespawnPlayer * 1000));

        PlayerSpawner.Instance.Respawn_ServerRpc(OwnerClientId);
    }

    public void DestroySelf() {
        Debug.Log("Player destroyed", this);

        Destroy(rope);
        Destroy(gameObject);
    }


    public NetworkObject networkObject;
    public Rigidbody body;
    public Transform lookVerticalPivot;
    public new Camera camera;
    public PlayerMovement playerMovement;
    public GunsHandler gunsHandler;
    public SpringJoint rope { get; set; }


    int health = 100;
    public bool isAlive => health > 0;

    [SerializeField] float suffocationSpeed;
    [SerializeField] float desuffocationSpeed;
    float suffocation;


    public Action OnStart = () => { };
    public Action OnUpdate = () => { };


    private void Awake() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Start() {
        //RegisterPlayer(this);

        if (IsOwner) {
            OnStart();
            camera.gameObject.SetActive(true);
        }
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
            if (rope.currentForce.magnitude > 2) suffocationApply = Mathf.Min(rope.currentForce.magnitude, 20) * suffocationSpeed;
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



}