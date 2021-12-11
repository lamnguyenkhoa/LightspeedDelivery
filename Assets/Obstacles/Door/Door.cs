using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door : MonoBehaviour
{
    public int foodBagsNeeded = 1;
    int foodCount = 0;
    Animator anim;

    private void Awake() {
        anim = GetComponent<Animator>();
    }

    public void IncreaseFoodCount()
    {
        foodCount++;

        if (foodCount >= foodBagsNeeded)
            anim.SetTrigger("Open");
    }
}
