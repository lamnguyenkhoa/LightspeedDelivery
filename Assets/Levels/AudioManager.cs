using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public AudioSource bgm;
    public AudioSource footstepSpeaker;

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

    private void Start()
    {
        bgm.Play();
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
}