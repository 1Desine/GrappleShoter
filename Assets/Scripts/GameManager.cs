using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour { 
    static public GameManager Instance { get; private set; }


    [SerializeField] List<Transform> spawnPoints;
    List<Player> playersList = new List<Player>();


    [Header("Game Settings")]
    [Min(0)] public float recoilModifier;


    private void Awake() {
        Instance = this;
    }
    


    public void RegisterPlayer(Player player) {
        playersList.Add(player);
    }
    public void UnregisterPlayer(Player player) {
        playersList.Remove(player);
    }


    public Vector3 GetRandomSpawnPoint() {
        return spawnPoints[Random.Range(0, spawnPoints.Count)].position;
    }

}
