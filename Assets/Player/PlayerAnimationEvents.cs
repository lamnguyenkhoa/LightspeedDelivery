using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public void PlayFootstep()
    {
        AudioManager.instance.PlayFootstep();
    }

    public void PlayLanding()
    {
        AudioManager.instance.PlayLanding();
    }
}