using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    public static GameAssets i
    {
        get
        {
            if (_i == null)
            {
                GameAssets x = Resources.Load<GameAssets>("GameAssets");

                _i = Instantiate(x);
            }
            return _i;
        }
    }

    //Have a nice day.
}