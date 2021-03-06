using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBag : MonoBehaviour
{
    public MainInstances mainInstances;
    public PlayerStats playerStats;
    [HideInInspector]
    public Vector3 shootDirection;

    [HideInInspector]
    public float shootForce;

    [HideInInspector]
    public Vector3 bonusFromPlayerMomentum;

    public float returnTime = 10f;
    private float timer = 0f;
    // This prevent foodbag get picked back right after shoot it
    private float timeBeforeAllowedRepickup = 1f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(shootDirection * shootForce + bonusFromPlayerMomentum, ForceMode.Impulse);
    }

    private void Update()
    {
        // Food bag return to player if not reached the deliverySpot
        // within time. For lore reason just say it has super GPS tracking stuff.
        timer += Time.deltaTime;
        if (timer > returnTime)
        {
            playerStats.foodbags += 1;
            Destroy(this.gameObject);
        }
    }

    // If player managed to touch foodbag, add it back immediately
    private void OnTriggerEnter(Collider other)
    {
        Player player = other.transform.GetComponent<Player>();
        if (player && timer > timeBeforeAllowedRepickup)
        {
            playerStats.foodbags += 1;
            Destroy(this.gameObject);
        }
    }
}