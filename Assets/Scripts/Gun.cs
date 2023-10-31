using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gun : MonoBehaviour {
    [SerializeField] GunUI gunUI;
    [SerializeField] Transform visualTransform;

    [SerializeField, Min(0)] float reloadTime;
    [SerializeField] uint damage;
    [SerializeField, Min(0)] float fireRate;
    float _fireRate { get { return fireRate / 60f / 100f; } }
    [SerializeField] float recoil;
    [SerializeField, Min(0)] int clipSize;
    int bulletsLoaded;

    float lastShotTime;
    bool reloading = false;
    float reloadProgressTime;


    private void Awake() {
        bulletsLoaded = clipSize;
        gunUI.SetLoadedBullets(bulletsLoaded, clipSize);
    }


    public void Shoot() {
        if (bulletsLoaded == 0) return;

        reloading = false;

        if (Time.time - lastShotTime > _fireRate) {
            lastShotTime = Time.time;

            bulletsLoaded--;

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
