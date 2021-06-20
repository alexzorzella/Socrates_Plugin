using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    Animator anim;
    public List<Transform> referencedObjects = new List<Transform>();
    float openDistance = 0.4F;

    private void Start()
    {
        GetComponent();
    }

    public void AddReferencedObject(Transform get)
    {
        referencedObjects.Add(get);
    }

    private void GetComponent()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        Open();
    }

    public Transform Closest()
    {
        List<Transform> temp = new List<Transform>();

        float currentDistance = float.MaxValue;
        Transform closest = null;

        foreach (var obj in referencedObjects)
        {
            if (obj != null)
            {
                float refDistance = Vector2.Distance(obj.position, transform.position);

                if (refDistance <= currentDistance)
                {
                    currentDistance = refDistance;
                    closest = obj;
                }

                temp.Add(obj);
            }
        }

        referencedObjects = temp;

        return closest;
    }

    public void Open()
    {
        if (referencedObjects.Count <= 0) { return; }

        anim.SetBool("inRange", Vector2.Distance(transform.position, Closest().position) < openDistance);

        if (anim.GetCurrentAnimatorClipInfo(0)[0].clip.name.Contains("closed"))
        {
            if ((transform.position.x - Closest().position.x) < 0)
            {
                Flip(-1);
            }
            else
            {
                Flip(1);
            }
        }
    }

    private void Flip(int x)
    {
        Vector3 scaler = transform.localScale;
        scaler.x = x;
        transform.localScale = scaler;
    }
}