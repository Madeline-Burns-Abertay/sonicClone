using UnityEngine;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioSource))]

public class bgm : MonoBehaviour
{
    AudioSource intro, loop;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        intro.Play();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
