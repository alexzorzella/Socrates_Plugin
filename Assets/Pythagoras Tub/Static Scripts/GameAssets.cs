using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAssets : MonoBehaviour
{
    private static GameAssets _i;

    private void Start()
    {
        Console i = Console.i;
        startTime = DateTime.Now.Ticks;
        DontDestroyOnLoad(gameObject);
    }

    private long startTime;
    public Texture Socrates;

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

    public Particle[] particleCatalogue;

    private GameObject GetParticle(string name, Vector2 position)
    {
        Particle p = Array.Find(particleCatalogue, part => part.name == name);

        if(p != null)
        {
            return Instantiate(p.particle, position, Quaternion.identity);
        }

        Debug.LogWarning($"Não tem uma quimica com esse nome, cheque o catalogo.");

        return null;
    }
    
    public static GameObject Particle(string name, Vector2 position)
    {
        return i.GetParticle(name, position);
    }

    //Have a nice day.
}

[System.Serializable]
public class Particle
{
    public string name;
    public GameObject particle;
}