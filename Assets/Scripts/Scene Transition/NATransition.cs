using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NATransition : MonoBehaviour
{
    public Animator anim;

    float delay = 0.5F;

    CanvasGroup group;

    private static NATransition _i;

    public static NATransition i
    {
        get
        {
            if (_i == null)
            {
                NATransition x = Resources.Load<NATransition>("Transition");
                _i = Instantiate(x);
            }

            return _i;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        group = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        group.interactable = IsTransitioning();
        group.blocksRaycasts = IsTransitioning();
    }

    public static bool IsTransitioning()
    {
        return i.anim.GetBool("trans");
    }

    public static void QuitGame()
    {
        i.Quit();
    }

    private void Quit()
    {
        StartCoroutine(QuitGameT());
    }

    private void T(string sceneName)
    {
        StartCoroutine(TransitionT(sceneName));
    }

    private void T(int sceneIndex)
    {
        StartCoroutine(TransitionT(sceneIndex));
    }

    public static void Transition(string sceneName)
    {
        i.T(sceneName);
    }
    
    public static void Transition(int sceneIndex)
    {
        i.T(sceneIndex);
    }

    IEnumerator TransitionT(string s)
    {
        anim.SetBool("trans", true);
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(s);
        anim.SetBool("trans", false);
    }

    IEnumerator QuitGameT()
    {
        anim.SetBool("trans", true);
        yield return new WaitForSeconds(delay);
        Application.Quit();
        anim.SetBool("trans", false);
    }

    IEnumerator TransitionT(int i)
    {
        anim.SetBool("trans", true);
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(i);
        anim.SetBool("trans", false);
    }

    public void Teleport(Transform playerTransform, Vector2 teleport)
    {
        StartCoroutine(TeleportDelay(playerTransform, teleport));
    }

    IEnumerator TeleportDelay(Transform playerTransform, Vector2 teleport)
    {
        anim.SetBool("trans", true);
        yield return new WaitForSeconds(delay);
        playerTransform.position = teleport;
        FindObjectOfType<PUNCamera>().transform.position = teleport;
        anim.SetBool("trans", false);
    }
}