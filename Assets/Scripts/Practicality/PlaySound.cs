using UnityEngine;

public class PlaySound : MonoBehaviour {
    public void PlayFlat(string sound) {
        AudioManager.i.Play(sound);
    }

    public void PlayWithNoise(string sound) {
        AudioManager.i.Play(sound, 0.8F, 1.2F);
    }
}