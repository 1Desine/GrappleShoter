using System.Collections;
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
            Reload();
            return;
        }

        reloading = false;
        gunUI.SetReloadProgress(reloadTime, 0);

        if (Time.time - lastShotTime > _fireRate) {
            lastShotTime = Time.time;

            bulletsLoaded--;
            gunsHandler.player.playerMovement.Push(-transform.forward * recoil * GameManager.Instance.recoilModifier);

            if (Physics.Raycast(gunsHandler.player.camera.transform.position, gunsHandler.player.camera.transform.forward, out RaycastHit hit)) {
                if (hit.rigidbody != null) {
                    if (hit.rigidbody.TryGetComponent(out PlayerObject playerHit)) {
                        playerHit.Damage_ServerRpc(damage, hit.point, 
                            (playerHit.transform.position - gunsHandler.player.transform.position).normalized * recoil * GameManager.Instance.recoilModifier, 
                            playerHit.OwnerClientId);
                    }
                }
                Debug.DrawLine(transform.position, hit.point, Color.red, 1);
            }

            gunUI.SetLoadedBullets(clipSize, bulletsLoaded);
        }
    }

    public void Reload() {
        if (reloading == true
            || bulletsLoaded == clipSize) return;

        reloading = true;
        reloadProgressTime = 0;
        StartCoroutine(ReloadCoroutine());
    }
    IEnumerator ReloadCoroutine() {
        reloadProgressTime += Time.deltaTime;
        if (reloadProgressTime > reloadTime) {
            reloading = false;
            bulletsLoaded = clipSize;

            gunUI.SetLoadedBullets(clipSize, bulletsLoaded);
            gunUI.SetReloadProgress(reloadTime, 0);
        }
        else if (reloading) {
            yield return new WaitForSeconds(Time.deltaTime);
            gunUI.SetReloadProgress(reloadTime, reloadProgressTime);
            StartCoroutine(ReloadCoroutine());
        }
    }

    public void Show() {
        visualTransform.gameObject.SetActive(true);
    }
    public void Hide() {
        visualTransform.gameObject.SetActive(false);
    }


}
