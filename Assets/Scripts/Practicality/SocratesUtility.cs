using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocratesUtility
{
    public static string CentsToString(int cents)
    {
        string centsDisplay = "";
        centsDisplay += "$";
        centsDisplay += (cents / 100).ToString();
        centsDisplay += ".";
        centsDisplay += (cents % 100).ToString("00");

        return centsDisplay;
    }

	public static string ConvertMoneyAmountToText(int price)
    {
        return ((float)price / 100F).ToString("0.00");
    }

    public static string TimeToTextMinutes(int convertFrom)
    {
        int seconds = convertFrom % 60;
        int minutes = (convertFrom - seconds) / 60;
        return $"{minutes}:{seconds.ToString("00")}";
    }
}