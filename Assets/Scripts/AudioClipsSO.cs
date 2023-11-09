using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class AudioClipsSO : ScriptableObject {

    public List<AudioClip> jump;
    public List<AudioClip> land;
    public List<AudioClip> attachRope;
    public List<AudioClip> detachRope;
    public List<AudioClip> footStep;
    public List<AudioClip> shoot;
    public List<AudioClip> die;
    public List<AudioClip> spawn;

    public enum Sound {
        Jump,
        Land,
        AttachRope,
        DetachRope,
        FootStep,
        Shoot,
        Die,
        Spawn,
    }

    public AudioClip GetRandomSound(Sound sound) {
        List<AudioClip> AudioClipList = sound switch {
            Sound.Jump => jump,
            Sound.Land => land,
            Sound.AttachRope => attachRope,
            Sound.DetachRope => detachRope,
            Sound.FootStep => footStep,
            Sound.Shoot => shoot,
            Sound.Die => die,
            Sound.Spawn =>spawn,
            _ => null
        };

        if(AudioClipList.Count== 0 ) {
            Debug.LogError("me. Intex out of range: " + sound);
            return null;
        }

        return AudioClipList[Random.Range(0, AudioClipList.Count - 1)];
    }

}
