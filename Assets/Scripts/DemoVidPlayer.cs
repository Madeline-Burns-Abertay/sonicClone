using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
[RequireComponent(typeof(VideoPlayer))]

public class DemoVidPlayer : MonoBehaviour
{
    VideoPlayer player;
    [SerializeField] List<VideoClip> demos;
    [SerializeField] float demoLength = 30f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GetComponent<VideoPlayer>();
        VideoClip demo = demos[Random.Range(0, demos.Count)];
        player.clip = demo;
        player.Play();
        StartCoroutine(backToTitle(demoLength));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator backToTitle(float length)
    {
        yield return new WaitForSeconds(length);
        SceneManager.LoadScene("TitleScreen");
    }
}
