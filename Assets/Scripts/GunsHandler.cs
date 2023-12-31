using System.Collections.Generic;
using UnityEngine;

public class GunsHandler : MonoBehaviour {
    public PlayerObject player;

    [SerializeField] List<Gun> gunsList;
    int selectedGunIndex = 0;

    private void Awake() {
        foreach (Gun gun in gunsList) {
            gun.Hide();
        }
        gunsList[selectedGunIndex].Show();
    }
    private void Start() {
        player.OnUpdate += Player_OnUpdate;
    }

    void Player_OnUpdate() {
        if (Input.GetMouseButton(0)) Shoot();
        if (Input.GetKeyDown(KeyCode.R)) Reload();
    }


    public void Shoot() => gunsList[selectedGunIndex].Shoot();
    public void Reload() => gunsList[selectedGunIndex].TryStartReload();


    public void ResetWeapons() {
        foreach (Gun gun in gunsList) {
            gun.ResetWeapon();
        }
    }

}
