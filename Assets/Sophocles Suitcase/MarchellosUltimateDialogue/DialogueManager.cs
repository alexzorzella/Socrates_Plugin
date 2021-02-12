using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public DS.DialogueSection currentSection;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI contentText;

    [Header("Options")]
    public Vector3 origin = new Vector3(0, -220F, 0);
    public float spacing = -45F;
    public GameObject dialogueChoice;
    public GameObject clickToContinue;

    private Animator anim;

    public Transform parentChoicesTo;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(FindObjectOfType<ZPlayer_Sharon>() != null)
        {
            AlexianInput input = FindObjectOfType<ZPlayer_Sharon>().input;

            if (input.playerInput.currentControlScheme == "Gamepad")
            {
                if (input.WestButtonDown() && Talking())
                {
                    ProceedToNext();
                }
            }
        }
    }

    public void StartDialogue(DS.DialogueSection start)
    {
        anim.SetBool("open", true);
        ClearAllOptions();
        currentSection = start;
        DisplayDialogue();
    }

    bool talking;

    public bool Talking()
    {
        return anim.GetBool("open");
    }

    public void ProceedToNext()
    {
        AudioManager.i.Play("blurb", 1.4F, 1.4F);

        if (currentSection.GetAction() != null && talking)
        {
            return;
        }

        if(currentSection.GetNextSection() != null)
        {
            currentSection = currentSection.GetNextSection();
            DisplayDialogue();
        } else
        {
            EndDialogue();
        }
    }

    public void DisplayDialogue()
    {
        if (currentSection == null)
        {
            EndDialogue();
            return;
        }

        bool monologue = typeof(DS.Monologue).IsInstanceOfType(currentSection);

        clickToContinue.SetActive(monologue);

        if (monologue)
        {
            //ClearAllOptions();
            //delete all of the current options
        }

        ClearAllOptions();

        contentText.text = "";

        StopAllCoroutines();
        StartCoroutine(DisplayText());
    }

    IEnumerator DisplayText()
    {
        if (currentSection.GetAction() != null)
        {
            currentSection.TriggerAction(true);
        }

        nameText.text = currentSection.GetSpeakerName();
        contentText.text = currentSection.GetTitle(); //Needs to access the sentences in the dialogue, going to be either the sentence or header

        contentText.ForceMeshUpdate();

        int totalVisibleCharacters = contentText.textInfo.characterCount;
        int counter = 0;

        talking = true;

        while (totalVisibleCharacters >= counter)
        {
            //contentText.ForceMeshUpdate();

            int visibleCount = counter % (totalVisibleCharacters + 1);
            contentText.maxVisibleCharacters = visibleCount;

            counter += 1;

            if (anim.GetBool("open"))
            {
                if (!currentSection.IsMonotone())
                {
                    AudioManager.i.PlayOnlyIfDone(currentSection.GetDialogueSound(), 0.8F, 1.2F);
                }
                else
                {
                    Sound s = Array.Find(AudioManager.i.sounds, sound => sound.name == currentSection.GetDialogueSound());

                    if (s != null)
                    {
                        AudioManager.i.PlayOnlyIfDone(currentSection.GetDialogueSound(), s.pitch, s.pitch);
                    }
                }
            }

            yield return new WaitForSeconds(currentSection.CharDelay());
        }

        if(currentSection.GetAction() != null)
        {
            currentSection.TriggerAction(false);
        }

        talking = false;

        if (typeof(DS.Choices).IsInstanceOfType(currentSection))
        {
            StartCoroutine(DisplayOptions());
            //display all of the options as a way to pick them
        }
    }

    public void EndDialogue()
    {
        anim.SetBool("open", false);
    }

    public void ClearAllOptions()
    {
        GameObject[] currentDialogueOptions = GameObject.FindGameObjectsWithTag("DialogueChoice");

        foreach (var entry in currentDialogueOptions)
        {
            entry.GetComponent<Animator>().SetBool("exit", true);
            Destroy(entry, 0.2F);
        }
    }

    public IEnumerator DisplayOptions()
    {
        int i = 0;

        DS.Choices choices = (DS.Choices)currentSection;

        foreach (var option in choices.choices)
        {
            GameObject s = Instantiate(dialogueChoice, Vector3.zero, Quaternion.identity);
            s.transform.SetParent(parentChoicesTo, false);

            //s.GetComponent<RectTransform>().transform.position = transform.position;

            //s.GetComponent<RectTransform>().transform.position = transform.position + origin + new Vector3(0, spacing * i, 0);

            DialogueOptionDisplay optionDisplayBehavior = s.GetComponent<DialogueOptionDisplay>();
            optionDisplayBehavior.SetParams(option.Item1, option.Item2);

            AudioManager.i.Play("blurb", 1.4F, 1.4F);

            yield return new WaitForSeconds(optionDisplayBehavior.AnimationLength());

            i++;
        }
    }
}