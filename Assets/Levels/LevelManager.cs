using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    // [HideInInspector]
    // public Player player;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
            // player = GameObject.Find("PlayerLloyd").GetComponent<Player>();
            // if (!player)
            //     player = GameObject.Find("Player").GetComponent<Player>();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
}