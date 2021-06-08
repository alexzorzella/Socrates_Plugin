using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScottsUtility : MonoBehaviour
{
    private static ScottsUtility _i;

    public static ScottsUtility i
    {
        get
        {
            if (_i == null)
            {
                ScottsUtility x = Resources.Load<ScottsUtility>("ScottsUtility");
                
                _i = Instantiate(x);
            }

            return _i;
        }
    }

    public static bool InRange(float x, float y, float range)
    {
        return (x + y) / 2 < range;
    }

    public GameObject[] nodes;

    private GameObject LoadNode(string nodename, Vector2 position, Quaternion angle, Color color, Vector2 size)
    {
        GameObject g = Array.Find(nodes, n => n.name == nodename);

        if(g != null)
        {
            GameObject n = Instantiate(g, position, angle);
            n.transform.localScale = size;
            n.GetComponentInChildren<SpriteRenderer>().color = color;

            return n;
        }

        return null;
    }

    public static GameObject CreateNode(string nodename, Vector2 position, Quaternion angle, Color color, Vector2 size)
    {
        return i.LoadNode(nodename, position, angle, color, size);
    }
}

public interface ScottUtilityCommand
{
    string Command();
}