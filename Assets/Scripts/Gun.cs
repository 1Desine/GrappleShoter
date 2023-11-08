using System.Collections;
using System.Threading.Tasks;
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

    public void TryStartReload() {
        if (reloading == true
            || bulletsLoaded == clipSize) return;

        reloading = true;
        reloadProgressTime = (float)bulletsLoaded / clipSize;
        Reloading();
    }
    async void Reloading() {
        reloadProgressTime += Time.deltaTime;
        if (reloading) {
            if (reloadProgressTime > reloadTime) {
                reloading = false;
                bulletsLoaded = clipSize;

                gunUI.SetReloadProgress(reloadTime, 0);
            }
            else {
                await Task.Delay((int)(Time.deltaTime * 1000));
                gunUI.SetReloadProgress(reloadTime, reloadProgressTime);
                Reloading();
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
