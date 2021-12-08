using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverySpotScript : MonoBehaviour
{
    private bool delivered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (delivered) return;

        if (other.GetComponent<FoodBagScript>())
        {
            delivered = true;
            LevelManager.instance.player.deliveredAmount += 1;
            Destroy(other.gameObject);
            Debug.Log("Delivered 1 foodbag");
            Destroy(this.gameObject);
        }
    }
}