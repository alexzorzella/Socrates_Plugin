using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEditor;
using System.Linq;

public class IntroFunc : MonoBehaviour
{
    DeltaCameraShake cameraShake;

    public Transform[] letters;

    public int letterFlipIndex = -1;
    public float lerpSpeed = 5F;
    public float lerpTime = 0;

    public void ResetLetters()
    {
        letterFlipIndex = -1;

        foreach (var letter in letters)
        {
            letter.transform.rotation = Quaternion.Euler(new Vector3(0, -90F, 0));
        }
    }

    public void EngageLetterScroll()
    {
        letterFlipIndex = 0;
        lerpTime = 0;
    }

    public void UpdateLetters()
    {
        if (letterFlipIndex != -1 && letterFlipIndex < letters.Length)
        {
            if(letters[letterFlipIndex].transform.rotation.eulerAngles.y == 0F)
            {
                letters[letterFlipIndex].transform.rotation = Quaternion.identity;
                letterFlipIndex++;
                lerpTime = 0;

                if (letterFlipIndex >= letters.Length)
                    return;
            }

            letters[letterFlipIndex].rotation = Quaternion.Lerp(letters[letterFlipIndex].rotation, Quaternion.identity, lerpTime);
            lerpTime += Time.deltaTime * lerpSpeed;
        }
    }

    private void Start()
    {
        cameraShake = Camera.main.GetComponent<DeltaCameraShake>();
        ResetLetters();
    }

    public void Shake()
    {
        cameraShake.Shake(Shakepedia.GetProfileClone(Shakepedia.MILD));
    }

    public void ShakeLight()
    {
        cameraShake.Shake(Shakepedia.GetProfileClone(Shakepedia.TAPER));
    }

    public TextMeshProUGUI animationNameDisplay;
    public Animator animator;

    public void UpdateAnimationText()
    {
        animationNameDisplay.text = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;
    }

    public void Update()
    {
        UpdateAnimationText();
        UpdateLetters();
    }

    public void PlaySound(string sound)
    {
        string[] sounds = sound.Split(',');

        foreach (var s in sounds)
        {
            AudioManager.i.Play(s);
        }
    }

    public void StopSound(string sound)
    {
        AudioManager.i.StopAllSources(sound, true);
    }

    public GameObject guts;
    public Transform guts_position;

    public void Guts()
    {
        Instantiate(guts, guts_position.position, Quaternion.identity);
    }

    public void LoadScene(string sceneName)
    {
        NATransition.Transition(sceneName);
    }
}