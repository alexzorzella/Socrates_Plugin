using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SmoothTransition : MonoBehaviour
{
    private Animator anim;

    public float delay;

    private void Start()
    {
        InitializeValues();
    }

    private void InitializeValues()
    {
        anim = GetComponent<Animator>();
    }

    public void Transition(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void Transition(int sceneIndex)
    {
        SceneManager.LoadScene(sceneIndex);
    }

    IEnumerator TransitionT(string s)
    {
        anim.SetBool("trans", true);
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(s);
    }

    IEnumerator TransitionT(int i)
    {
        anim.SetBool("trans", true);
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(i);
    }
}