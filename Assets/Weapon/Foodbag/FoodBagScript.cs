using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBagScript : MonoBehaviour
{
    [HideInInspector]
    public Vector3 shootDirection;

    [HideInInspector]
    public float shootForce;

    [HideInInspector]
    public Vector3 bonusFromPlayerMomentum;

    public float returnTime = 10f;
    private float timer = 0f;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(shootDirection * shootForce + bonusFromPlayerMomentum, ForceMode.Impulse);
    }

    private void Update()
    {
        // Foog bad return to player if not reached the deliverySpot
        // within time. For lore reason just say it has super GPS tracking stuff.
        timer += Time.deltaTime;
        if (timer > returnTime)
        {
            LevelManager.instance.player.currentFoodBag += 1;
            Destroy(this.gameObject);
        }
    }
}