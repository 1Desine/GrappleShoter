using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Gun : MonoBehaviour {
    [SerializeField] GunUI gunUI;
    [SerializeField] Transform visualTransform;

    [SerializeField, Min(0)] float reloadTime;
    [SerializeField, Min(0)] int damage;
    [SerializeField, Min(0)] float fireRate;
    float _fireRate { get { return fireRate / 60f / 100f; } }
    [SerializeField] float recoil;
    [SerializeField, Min(0)] int clipSize;
    int bulletsLoaded;

    float lastShotTime;
    bool reloading = false;
    float reloadProgressTime;


    GunsHandler gunsHandler;


    private void Awake() {
        ResetWeapon();

        gunsHandler = transform.parent.GetComponent<GunsHandler>();
    }
    public void ResetWeapon() {
        bulletsLoaded = clipSize;
        gunUI.SetLoadedBullets(bulletsLoaded, clipSize);
    }



    public void Shoot() {
        if (bulletsLoaded == 0) {
            TryStartReload();
            return;
        }

        reloading = false;

        if (Time.time - lastShotTime > _fireRate) {
            lastShotTime = Time.time;

            bulletsLoaded--;
            gunUI.SetLoadedBullets(clipSize, bulletsLoaded);

            if (Physics.Raycast(gunsHandler.player.camera.transform.position, gunsHandler.player.camera.transform.forward, out RaycastHit hit)) {
                if (hit.rigidbody != null) {
                    if (hit.rigidbody.TryGetComponent(out PlayerObject playerHit)) {
                        playerHit.Damage_ServerRpc(damage, playerHit.OwnerClientId, gunsHandler.player.camera.transform.forward * recoil * GameManager.Instance.recoilModifier, hit.point);
                    }
                    else if (hit.rigidbody.TryGetComponent(out NetworkObject netObj)) {
                        PushTarger_ServerRpc(netObj, gunsHandler.player.camera.transform.forward * recoil * GameManager.Instance.recoilModifier, hit.point);
                    }
                }
                Debug.DrawLine(transform.position, hit.point, Color.red, 1);
            }

            gunsHandler.player.playerMovement.Push(-transform.forward * recoil * GameManager.Instance.recoilModifier);

            SoundManager.Instance.PlaySoundAtPoint_ServerRpc(AudioClipsSO.Sound.Shoot, transform.position);

            SpawnManager.Instance.SpawnTrace_ServerRpc(transform.position, gunsHandler.player.camera.transform.forward);
        }
    }
    [ServerRpc(RequireOwnership = false)]
    void PushTarger_ServerRpc(NetworkObjectReference reference, Vector3 force, Vector3 point) {
        reference.TryGet(out NetworkObject netObj);
        netObj.TryGetComponent(out Rigidbody body);

        body.AddForceAtPosition(force, point);
    }
    public void TryStartReload() {
        if (reloading == true
            || bulletsLoaded == clipSize) return;

        reloading = true;
        reloadProgressTime = (float)bulletsLoaded / clipSize;
        StartCoroutine(Reloading());
    }
    IEnumerator Reloading() {
        reloadProgressTime += Time.deltaTime;
        if (reloading) {
            if (reloadProgressTime > reloadTime) {
                reloading = false;
                bulletsLoaded = clipSize;

                gunUI.SetReloadProgress(reloadTime, 0);
            }
            else {
                yield return new WaitForSeconds(Time.deltaTime);
                gunUI.SetReloadProgress(reloadTime, reloadProgressTime);
                StartCoroutine(Reloading());
            }
        }
        else {
            Debug.Log("Loaded bullets: " + bulletsLoaded);
            bulletsLoaded = (int)(clipSize * reloadProgressTime / reloadTime);
            gunUI.SetReloadProgress(reloadTime, 0);
        }
        gunUI.SetLoadedBullets(clipSize, bulletsLoaded);
    }

    public void Show() {
        visualTransform.gameObject.SetActive(true);
    }
    public void Hide() {
        visualTransform.gameObject.SetActive(false);
    }


}
