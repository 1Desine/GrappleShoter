using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GunUI : MonoBehaviour {

    [SerializeField] Image bulletsLoadedImage;
    [SerializeField] Image ReloadProgressImage;


    public void SetLoadedBullets(int clipSize, int loadedBullets) =>bulletsLoadedImage.fillAmount = loadedBullets / (float)clipSize;
    
    public void SetReloadProgress(float reloadTime, float reloadProgressTime) => ReloadProgressImage.fillAmount = reloadProgressTime / reloadTime;

}
