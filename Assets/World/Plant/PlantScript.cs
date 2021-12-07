using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlantScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        FPSController player = other.transform.GetComponent<FPSController>();
        if (player)
        {
            player.nPlantInRange += 1;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        FPSController player = other.transform.GetComponent<FPSController>();
        if (player)
        {
            player.nPlantInRange -= 1;
        }
    }
}