using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    private static GameManager pInstance = null;

    public GameStats stats;

    public static GameManager Instance()
    {
        if (pInstance == null)
        {
            //Debug.Log("GameManager Awake");
            // The very first time someone calls Instance() we populate pInstance
            pInstance = new GameManager();
            // and call its InitGame
            pInstance.InitGame();
        }
        return pInstance;
    }

    // Note to dad: this is called once and only once in the begginning of the game
    void InitGame()
    {
        //Debug.Log("InitGame");
        stats = new GameStats();
    }
}