using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine;

public class SpawnManager : NetworkBehaviour {
    static public SpawnManager Instance { get; private set; }

    [SerializeField] PlayerObject playerPrefab;
    [SerializeField] BulletTrace bulletTrace;


    Dictionary<ulong, PlayerObject> playersDictionary = new Dictionary<ulong, PlayerObject>();


    private void Awake() {
        Instance = this;
    }

    private void Start() {
        if (NetworkManager.Singleton == null) return;

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

        SoundManager.Instance.PlaySoundAtPoint_ServerRpc(AudioClipsSO.Sound.Spawn, player.transform.position);
    }
    public void RegisterPlayer(PlayerObject player) {
        playersDictionary.Add(player.OwnerClientId, player);
    }
    public void UnregisterPlayer(PlayerObject player) {
        playersDictionary.Remove(player.OwnerClientId);
    }



    [ServerRpc(RequireOwnership = false)]
    public void SpawnTrace_ServerRpc(Vector3 position, Vector3 direction) {
        BulletTrace trace = Instantiate(bulletTrace);

        trace.bullet.transform.localPosition = position;
        trace.direction = direction;
        trace.NetworkObject.Spawn();

        Destroy(trace.gameObject, 1);
    }
}
