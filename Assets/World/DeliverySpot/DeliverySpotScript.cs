using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DeliverySpotScript : MonoBehaviour
{
    public UnityEvent OnFoodDelivered;
    private bool delivered = false;

    private void OnTriggerEnter(Collider other)
    {
        if (delivered) return;

        if (other.GetComponent<FoodBagScript>())
        {
            delivered = true;
            FoodDelivered();
            // LevelManager.instance.player.deliveredAmount += 1;
            Destroy(other.gameObject);
            Debug.Log("Delivered 1 foodbag");
            // Destroy(this.gameObject);
        }
    }

    public void FoodDelivered()
    {
        OnFoodDelivered?.Invoke();
    }
}