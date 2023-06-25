using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class IncrementWithOverflow
{
    public static int Run(int currentInd, int totalCount, int change)
    {
        int result = currentInd + change;

        if (result >= totalCount)
        {
            result = 0;
        }
        else if (result < 0)
        {
            result = totalCount - 1;
        }

        return result;
    }
}