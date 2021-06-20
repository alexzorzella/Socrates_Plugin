using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AddToDoor : MonoBehaviour
{
    private void Start()
    {
        AddToDoors();
    }

    private void AddToDoors()
    {
        Door[] doors = FindObjectsOfType<Door>();

        foreach (var door in doors)
        {
            door.AddReferencedObject(transform);
        }
    }
}