using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level : MonoBehaviour {

    public static Level Instance { get; private set; }

    [SerializeField] List<Transform> spawnPoints;

    private void Awake() {
        Instance = this;
    }

    public Vector3 GetRandomSpawnPoint() {
        return spawnPoints[Random.Range(0, spawnPoints.Count)].position;
    }

}
