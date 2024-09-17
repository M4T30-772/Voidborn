using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIAudio : MonoBehaviour
{
    [SerializeField] private AudioClip hover; 
    [SerializeField] private AudioClip click; 
    [SerializeField] private AudioClip mainMenuMusic; 
    
    private AudioSource audioSource;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();

        if (mainMenuMusic != null && audioSource != null)
        {
            audioSource.clip = mainMenuMusic;
            audioSource.loop = true; 
            audioSource.Play();
        }
    }

    public void SoundOnHover()
    {
        if (hover != null && audioSource != null)
        {
            audioSource.PlayOneShot(hover);
        }
    }

    public void SoundOnClick()
    {
        if (click != null && audioSource != null)
        {
            audioSource.PlayOneShot(click);
        }
    }
    
    private void OnDestroy()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }
}
