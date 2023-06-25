using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScottsUtility
{
    public static bool InRange(float x, float y, float range)
    {
        return (x + y) / 2 < range;
    }

    public static string CentsToString(int cents)
    {
        string centsDisplay = "";
        centsDisplay += "$";
        centsDisplay += (cents / 100).ToString();
        centsDisplay += ".";
        centsDisplay += (cents % 100).ToString("00");

        return centsDisplay;
    }
}