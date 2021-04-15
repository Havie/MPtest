using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[RequireComponent(typeof(AudioSource))]

public class SoundHelper : MonoBehaviour
{
    [SerializeField] AudioClip clip;


    public void PlayAudio()
    {
        AudioSource.PlayClipAtPoint(clip, new Vector3(0, 0, 0));
    }

}
