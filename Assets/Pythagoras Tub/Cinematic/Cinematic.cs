using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cinematic : MonoBehaviour
{
    public string cinematicName;
    public GameObject[] referencedObjects;
    Animator anim;
    bool hasTriggered;
    public bool enableOnPlayerJoin;

    public Transform skipPromptTransform;

    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if(enableOnPlayerJoin)
        {
            EnableCutsceneOnPlayerJoin();
        }

        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("cutscene") && Input.GetKeyDown(KeyCode.Space))
        {
            AudioManager.i.Play("enter_game");
            anim.Play(anim.GetCurrentAnimatorClipInfo(0)[0].clip.name, 0, 0.99F);
        }
    }

    public void ShakeCamera()
    {
        CameramanTimothy.GetShake().Shake(Shakepedia.GetProfileClone(Shakepedia.MILD));
    }

    public void EnableCutsceneOnPlayerJoin()
    {
        if (FindObjectOfType<PlayerController>() != null && !hasTriggered)
        {
            hasTriggered = true;
            EnableCutscene();
        }
    }

    public void EnableCutscene()
    {
        anim.SetTrigger("cutscene");
    }

    public void SpawnObjectSplitWithPercentageSign(string name)
    {
        string objName = "";
        string spawnName = "";

        if(name.Split('%')[0] == null || name.Split('%')[0] == null)
        {
            return;
        }

        objName = name.Split('%')[0];
        spawnName = name.Split('%')[1];
        
        Instantiate(GetGetObjectFromName(objName), GetGetObjectFromName(spawnName).transform.position, Quaternion.identity);
    }

    public void SetCameraTarget(string name)
    {
        if(FindObjectOfType<CameramanTimothy>() == null)
        {
            Debug.Log("Camera not found.");
        }
        else
        {
            FindObjectOfType<CameramanTimothy>().SetTargetWithTransform(GetGetObjectFromName(name).transform);
        }
    }
    
    public void SetTargetTag(string name)
    {
        FindObjectOfType<CameramanTimothy>().SetTargetWithTag(name);
    }

    private GameObject GetGetObjectFromName(string name)
    {
        GameObject g = Array.Find(referencedObjects, ob => ob.name == name);

        if(g != null)
        {
            return g;
        }
        else
        {
            Debug.Log($"{name} not found");
            return null;
        }
    }

    public bool CurrentlyPlaying()
    {
        return anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("cutscene");
    }

    public static Cinematic GetByName(string inputName)
    {
        Cinematic[] objs = FindObjectsOfType<Cinematic>();

        Cinematic g = Array.Find(objs, o => o.cinematicName == inputName);

        if(g != null)
        {
            return g;
        }

        Debug.LogError($"Não achei segmento com nóme {inputName}.");
        return null;
    }
}