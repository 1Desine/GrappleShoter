using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;

public class PlayerObject : NetworkBehaviour {


    public static Dictionary<ulong, PlayerObject> playersDictionary = new Dictionary<ulong, PlayerObject>();
    private void OnPlayerConnected() {
        
    }
    public static void RegisterPlayer(PlayerObject player) {
        playersDictionary.Add(player.OwnerClientId, player);
    }
    public static void UnregisterPlayer(PlayerObject player) {
        playersDictionary.Remove(player.OwnerClientId);
    }
    [ServerRpc(RequireOwnership = false)]
    public void Spawn_ServerRpc(ulong id, string name) {
        PlayerObject player = Instantiate(GameManager.Instance.playerPrefab);
        player.transform.position = Level.Instance.GetRandomSpawnPoint();
        player.name = $"Player {id}, {name ?? "guest"}";

        player.networkObject.SpawnWithOwnership(id);

        RegisterPlayer(player);
    }
    [ServerRpc(RequireOwnership = false)]
    public void DestroySelf_ServerRpc() {
        networkObject.Despawn();
        UnregisterPlayer(this);
    }
    bool despawnInstructionHasBeenSent;
    public async void Die() {
        if (despawnInstructionHasBeenSent) return;
        despawnInstructionHasBeenSent = true;

        Destroy(rope);
        body.freezeRotation = false;
        float randomFloat = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
        Vector3 randomV3 = new Vector3(Mathf.Cos(randomFloat), 0, Mathf.Sin(randomFloat));
        body.AddForceAtPosition(transform.position, randomV3 * 100);

        await Task.Delay((int)(GameManager.Instance.timeToDespawnPlayer * 1000));

        Spawn_ServerRpc(OwnerClientId, null);
        DestroySelf_ServerRpc();
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
