using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LloydAnimationEvents : MonoBehaviour
{
    public void PlayFootstep()
    {
        AudioManager.instance.PlayFootstep();
    }
}