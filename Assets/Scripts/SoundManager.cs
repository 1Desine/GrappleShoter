using Unity.Netcode;
using UnityEngine;

public class SoundManager : MonoBehaviour {
    static public SoundManager Instance { get; private set; }

    [SerializeField] AudioClipsSO AudioClipsSO;


    private void Awake() {
        Instance = this;
        DontDestroyOnLoad(this);
    }

    [ServerRpc(RequireOwnership = false)]
    public void PlaySoundAtPoint_ServerRpc(AudioClipsSO.Sound sound, Vector3 point) {
        PlaySoundAtPoint_ClientRpc(AudioClipsSO.GetRandomSound(sound), point);
    }
    [ClientRpc]
    void PlaySoundAtPoint_ClientRpc(AudioClip clip, Vector3 point) {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, point);
    }





}
