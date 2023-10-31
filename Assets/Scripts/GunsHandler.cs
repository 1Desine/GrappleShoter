using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunsHandler : MonoBehaviour {


    [SerializeField] List<Gun> gunsList;
    int selectedGunIndex = 0;

    private void Awake() {
        foreach (Gun gun in gunsList) {
            gun.Hide();
        }
        gunsList[selectedGunIndex].Show();

    }

    private void Update() {

        if (Input.GetMouseButton(0)) Shoot();
        if (Input.GetKeyDown(KeyCode.R)) Reload();




    }


    public void Shoot() => gunsList[selectedGunIndex].Shoot();
    public void Reload() => gunsList[selectedGunIndex].Reload();




}
