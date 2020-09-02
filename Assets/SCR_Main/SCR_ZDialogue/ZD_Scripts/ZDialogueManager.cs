using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZDialogueManager : MonoBehaviour
{
    public string replaceText = "#name";
    public string playerName;

    public Text nameText;
    public Text dialogueText;

    public Animator animator;

    public DialogueMessage currentMessage;

    public GameObject choiceBox;
    public Vector3 startButtonPositon;
    private Vector3 currentChoiceButtonPositon;
    public Vector3 choiceButtonOffset;
    public Transform instantiateReference;
    
    public static readonly DialogueMessage TERMINATING = new DialogueMessage("");

    void Start()
    {
        StartDialogue();
    }

    public void StartDialogue()
    {
        animator.SetBool("IsOpen", true);
        currentMessage = new BridgeGuard().Create();
        DisplaySentence();
    }

    private void DisplaySentence()
    {
        StopAllCoroutines();
        StartCoroutine(TypeSentence());
    }

    IEnumerator PresentChoices()
    {
        StartCoroutine(DestroyCurrentUI());

        currentChoiceButtonPositon = startButtonPositon;

        for (int i = 0; i < currentMessage.choices.Count; i++)
        {
            GameObject localChoiceButton = Instantiate(choiceBox, instantiateReference.position + currentChoiceButtonPositon, Quaternion.identity);
            localChoiceButton.transform.SetParent(FindObjectOfType<Canvas>().transform);
            currentChoiceButtonPositon += choiceButtonOffset;
            localChoiceButton.GetComponent<ChoiceOption>().option = currentMessage.choices[i];
            localChoiceButton.GetComponentInChildren<Text>().text = currentMessage.choices[i].message;

            while (!localChoiceButton.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Enter"))
            {
                yield return new WaitForEndOfFrame();
            }

            AnimatorClipInfo[] animations = localChoiceButton.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0);
            float clipLength = animations[0].clip.length;
            yield return new WaitForSeconds(clipLength);
        }
    }

    IEnumerator TypeSentence()
    {
        StartCoroutine(DestroyCurrentUI());

        while (GameObject.FindGameObjectsWithTag("Choice").Length > 0)
        {
            yield return new WaitForEndOfFrame();
        }

        dialogueText.text = "";

        string updatedMessage = currentMessage.message;
        updatedMessage = updatedMessage.Replace(replaceText, playerName);

        foreach (char letter in updatedMessage.ToCharArray())
        {
            dialogueText.text += letter;

            yield return new WaitForSeconds(0.02F);
        }

        StartCoroutine(PresentChoices());
    }

    public void PickedChoice(Choice choice)
    {
        this.currentMessage = choice.nextMessage;

        if (this.currentMessage == TERMINATING)
        {
            animator.SetBool("IsOpen", false);
        }

        StopAllCoroutines();
        StartCoroutine(TypeSentence());
    }

    IEnumerator DestroyCurrentUI()
    {
        GameObject[] allChoiceButtons = GameObject.FindGameObjectsWithTag("Choice");

        if (allChoiceButtons != null)
        {
            for (int i = 0; i < allChoiceButtons.Length; i++)
            {
                allChoiceButtons[i].GetComponent<ChoiceOption>().DestroyChoiceButton();

                yield return new WaitForEndOfFrame();
            }
        }
    }
}