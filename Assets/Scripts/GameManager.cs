using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour { 
    static public GameManager Instance { get; private set; }


    [SerializeField] List<Transform> spawnPoints;
    public Dictionary<ulong,Player> playersList = new Dictionary<ulong, Player>();


    [Header("Game Settings")]
    [Min(0)] public float recoilModifier;


    private void Awake() {
        Instance = this;

        StartCoroutine(SpawnPlayers_Coroutine());
    }



    public void RegisterPlayer(Player player) {
        playersList.Add(player.OwnerClientId, player);
    }
    public void UnregisterPlayer(Player player) {
        playersList.Remove(player.OwnerClientId);
    }


    public Vector3 GetRandomSpawnPoint() {
        return spawnPoints[Random.Range(0, spawnPoints.Count)].position;
    }

    IEnumerator SpawnPlayers_Coroutine() {
        yield return new WaitForSeconds(1);

        


        StartCoroutine(SpawnPlayers_Coroutine());
    }

}
