using System;
using System.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

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

        SoundManager.Instance.PlaySoundAtPoint_ServerRpc(AudioClipsSO.Sound.Die, transform.position);


        await Task.Delay((int)(GameManager.Instance.timeToDespawnPlayer * 1000));

        SpawnManager.Instance.Respawn_ServerRpc(OwnerClientId);
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

    [SerializeField] float suffocationTimeDelay;
    [SerializeField] float suffocationSpeed;
    [SerializeField] float desuffocationSpeed;
    float lastBreathTime;
    float suffocation01;


    public Action OnStart = () => { };
    public Action OnUpdate = () => { };
    public Action OnFixedUpdate = () => { };

    private void OnEnable() {
        SettingsManager.Instance.OnFovChanged += SetFov;
        SetFov();
    }
    private void OnDisable() {
        SettingsManager.Instance.OnFovChanged -= SetFov;
    }
    private void Start() {
        if (IsOwner == false) return;
        OnStart();
        camera.gameObject.SetActive(true);
    }
    void SetFov() => camera.fieldOfView = SettingsManager.Instance.playerSettingsSO.fov;

    private void Update() {
        if (IsOwner == false) return;

        OnUpdate();

        if (Input.GetKeyDown(KeyCode.LeftAlt)) Die();
        if (Input.GetKeyDown(KeyCode.LeftAlt)) health = 0;

        UpdateSuffocation();
    }

    public void SetCameraFov(float fov) => camera.fieldOfView = fov;
    private void FixedUpdate() {
        OnFixedUpdate();
    }

    void UpdateSuffocation() {
        if (rope == null) lastBreathTime = Time.time;

        suffocation01 = Mathf.Clamp01(suffocation01 +
            (Time.time - lastBreathTime > suffocationTimeDelay ? suffocationSpeed : -desuffocationSpeed)
            * Time.deltaTime);

        PlayerUI.Instance?.SetSuffocation(suffocation01);
    }


    [ServerRpc(RequireOwnership = false)]
    public void Damage_ServerRpc(int damage, ulong playerId, Vector3 force, Vector3 point) {
        Damage_ClientRpc(damage, playerId,  force,  point);
    }
    [ClientRpc]
    public void Damage_ClientRpc(int damage,  ulong playerId, Vector3 force, Vector3 point) {
        if (OwnerClientId != playerId) return;

        if (health - damage <= 0 && isAlive) Die();
        health -= damage;

        body.AddForceAtPosition(force, point);
    }



}
