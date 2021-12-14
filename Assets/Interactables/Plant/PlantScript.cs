using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.transform.GetComponent<Player>();
        if (player)
        {
            player.nPlantInRange += 1;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Player player = other.transform.GetComponent<Player>();
        if (player)
        {
            player.nPlantInRange -= 1;
        }
    }
}