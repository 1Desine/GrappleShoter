using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour {
    static public PlayerSpawner Instance { get; private set; }

    [SerializeField] PlayerObject playerPrefab;


    Dictionary<ulong, PlayerObject> playersDictionary = new Dictionary<ulong, PlayerObject>();


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        Spawn_ServerRpc(NetworkManager.Singleton.LocalClientId);
        Debug.Log("client connected: " + NetworkManager.Singleton.LocalClientId);
    }



    [ServerRpc(RequireOwnership = false)]
    public void Respawn_ServerRpc(ulong clientId) {
        PlayerObject player = playersDictionary[clientId];
        player.DestroySelf();
        UnregisterPlayer(player);

        Spawn_ServerRpc(clientId);
    }
    [ServerRpc(RequireOwnership = false)]
    public void Spawn_ServerRpc(ulong clientId) {
        PlayerObject player = Instantiate(playerPrefab);
        player.transform.position = Level.Instance.GetRandomSpawnPoint();
        player.name = $"Player {clientId}, {name ?? "guest"}";

        player.networkObject.SpawnWithOwnership(clientId);
        RegisterPlayer(player);
    }


    public void RegisterPlayer(PlayerObject player) {
        playersDictionary.Add(player.OwnerClientId, player);
    }
    public void UnregisterPlayer(PlayerObject player) {
        playersDictionary.Remove(player.OwnerClientId);
    }
}
