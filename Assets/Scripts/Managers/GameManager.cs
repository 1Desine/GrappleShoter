using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour { 
    static public GameManager Instance { get; private set; }


    [Header("Game Settings")]
    [Min(0)] public float recoilModifier;
    public float timeToDespawnPlayer;


    private void Awake() {
        Instance = this;
    }


    private void Start() {
        
    }


}
