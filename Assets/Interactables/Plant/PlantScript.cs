using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlantScript : MonoBehaviour
{
    public UnityEvent OnPlayerEntered;
    public UnityEvent OnPlayerExited;
    private void OnTriggerEnter(Collider other)
    {
        OnPlayerEntered?.Invoke();
        // Player player = other.transform.GetComponent<Player>();
        // if (player)
        // {
        //     player.nPlantInRange += 1;
        // }
    }

    private void OnTriggerExit(Collider other)
    {
        OnPlayerExited?.Invoke();
        // Player player = other.transform.GetComponent<Player>();
        // if (player)
        // {
        //     player.nPlantInRange -= 1;
        // }
    }
}