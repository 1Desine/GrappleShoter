using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;

public class Player : NetworkBehaviour {
    

    public Transform lookVerticalPivot;
    public new Camera camera;
    public Rigidbody body;
    public SpringJoint springJoint;
    public PlayerSettings playerSettings;
    public PlayerMovement playerMovement;
    public GunsHandler gunsHandler;
    public Transform capsule;

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



    public void Damage(int damage) {
        health -= damage;
        if (health <= 0) Die();
    }

    private void Die() {
        Debug.Log("me dead bro", this);
        StartCoroutine(SpawnCoroutine());
    }


    void Hide() {
        capsule.gameObject.SetActive(false);

        body.position = Vector3.zero;
        body.velocity = Vector3.zero;
        body.useGravity = false;
    }
    void Show() {
        capsule.gameObject.SetActive(true);

        body.useGravity = true;
    }

    IEnumerator SpawnCoroutine() {
        yield return new WaitForSeconds(3);
        Hide();
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
