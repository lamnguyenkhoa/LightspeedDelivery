using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;

    [HideInInspector]
    public FPSController player;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Start()
    {
        player = GameObject.Find("PlayerLloyd").GetComponent<FPSController>();
        if (!player)
            player = GameObject.Find("Player").GetComponent<FPSController>();
    }
}