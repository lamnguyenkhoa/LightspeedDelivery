using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeliverySpotScript : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<FoodBagScript>())
        {
            Destroy(other.gameObject);
            Debug.Log("Delivered 1 foodbag");
        }
    }
}