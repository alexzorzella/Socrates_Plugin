using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour {
    static SettingsMenu _i;
    public static readonly float alphaLerpSpeed = 10F;

    public bool visible;
    public AudioMixer sfx;
    public AudioMixer music;
    public Slider sfxSlider;
    public Slider musicSlider;
    CanvasGroup group;

    public static SettingsMenu i {
        get {
            if (_i == null) {
                var x = Resources.Load<SettingsMenu>("SettingsMenu");

                _i = Instantiate(x);
            }

            return _i;
        }
    }

    void Start() {
        group = GetComponentInChildren<CanvasGroup>();

        sfxSlider.maxValue = 20F;
        sfxSlider.minValue = -80F;

        musicSlider.maxValue = 20F;
        musicSlider.minValue = -80F;

        var sfxVolume = 0F;
        sfx.GetFloat("Volume", out sfxVolume);
        var musicVolume = 0F;
        music.GetFloat("Volume", out musicVolume);

        sfxSlider.value = sfxVolume;
        musicSlider.value = musicVolume;
    }

    void Update() {
        group.alpha = Mathf.Lerp(group.alpha, visible ? 1 : 0, Time.deltaTime * alphaLerpSpeed);

        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    void ToggleSettings(bool toggle) {
        visible = toggle;
    }

    public static void Static_ToggleSettingsMenuVisibility(bool toggle) {
        i.ToggleSettings(toggle);
    }

    public static void Static_ToggleSwitchMenuVisibility() {
        i.ToggleSettings(!i.visible);
    }

    public void _ChangeSFXVolume(float volume) {
        sfx.SetFloat("Volume", volume);
    }

    public void _ChangeMusicVolume(float volume) {
        music.SetFloat("Volume", volume);
    }

    public void QuitGame() {
        visible = false;

        if (NATransition.IsTransitioning()) return;

        AudioManager.i.Play("enter_game");

        NATransition.QuitGame();
    }

    public void MainMenu() {
        visible = false;

        if (NATransition.IsTransitioning()) return;

        AudioManager.i.Play("enter_game");

        NATransition.Transition("SaveSelect");
    }
}