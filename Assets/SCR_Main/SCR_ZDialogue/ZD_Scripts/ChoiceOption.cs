using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChoiceOption : MonoBehaviour
{
    public Choice option;

    public void ChooseOption()
    {
        FindObjectOfType<ZDialogueManager>().PickedChoice(option);
    }

    public void DestroyChoiceButton()
    {
        StartCoroutine(DestroyChoice());
    }

    IEnumerator DestroyChoice()
    {
        gameObject.GetComponent<Animator>().SetTrigger("exit");

        while (!gameObject.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("Exit"))
        {
            yield return new WaitForEndOfFrame();
        }

        AnimatorClipInfo[] animations = gameObject.GetComponent<Animator>().GetCurrentAnimatorClipInfo(0);
        float clipLength = animations[0].clip.length;
        yield return new WaitForSeconds(clipLength);

        Destroy(gameObject);
    }
}
