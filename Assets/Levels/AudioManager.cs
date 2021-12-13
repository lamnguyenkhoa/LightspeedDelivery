using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource bgm;
    public AudioSource footstepSpeaker;
    public AudioSource foodSplatSpeaker;
    public AudioSource landingSpeaker;
    public AudioSource dashSpeaker;

    public AudioClip[] footstepClips;

    private void Awake()
    {
        if (!instance)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void PlayFootstep()
    {
        float randomPitch = Random.Range(0.8f, 1.2f);
        float randomVolume = Random.Range(0.8f, 1f);
        footstepSpeaker.pitch = randomPitch;
        footstepSpeaker.volume = randomVolume;

        int randomClipIndex = Random.Range(0, footstepClips.Length);
        footstepSpeaker.clip = footstepClips[randomClipIndex];

        footstepSpeaker.Play();
    }

    public void PlayFoodSplat()
    {
        float randomPitch = Random.Range(0.8f, 1.2f);
        float randomVolume = Random.Range(0.8f, 1f);
        foodSplatSpeaker.pitch = randomPitch;
        foodSplatSpeaker.volume = randomVolume;
        foodSplatSpeaker.Play();
    }

    public void PlayLanding()
    {
        float randomPitch = Random.Range(0.8f, 1.2f);
        float randomVolume = Random.Range(0.8f, 1f);
        landingSpeaker.pitch = randomPitch;
        landingSpeaker.volume = randomVolume;
        landingSpeaker.Play();
    }

    public void PlayDash()
    {
        float randomPitch = Random.Range(0.8f, 1.2f);
        float randomVolume = Random.Range(0.8f, 1f);
        dashSpeaker.pitch = randomPitch;
        dashSpeaker.volume = randomVolume;
        dashSpeaker.Play();
    }
}