using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Mailbox : MonoBehaviour
{
    public MainInstances mainInstances;
    public UnityEvent OnFoodDelivered;
    private bool delivered = false;
    public GameObject distanceIndicator;
    private TextMesh distanceText;
    private Transform player;
    private Vector3 baseScale;
    public float scaleFactor = 0.01f;

    private void Start()
    {
        distanceText = distanceIndicator.GetComponent<TextMesh>();
        player = mainInstances.player.transform;
        baseScale = distanceIndicator.transform.localScale;
    }

    private void Update()
    {
        if (!delivered)
        {
            float distance = Vector3.Distance(player.position, transform.position);
            if (distance > 250f)
                return;

            distanceText.text = distance.ToString("F0") + "m";
            distanceIndicator.transform.localScale = baseScale * (distance / 10) * scaleFactor;

            distanceIndicator.transform.LookAt(player);
            distanceIndicator.transform.forward = -distanceIndicator.transform.forward;
        }
    }

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
        Destroy(distanceIndicator);
    }
}