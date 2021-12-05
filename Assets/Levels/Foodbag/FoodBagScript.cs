using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodBagScript : MonoBehaviour
{
    [HideInInspector]
    public Vector3 shootDirection;

    [HideInInspector]
    public float shootForce;

    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.AddForce(shootDirection * shootForce, ForceMode.Impulse);
    }
}