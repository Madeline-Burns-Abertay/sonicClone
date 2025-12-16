using UnityEngine;
using System.Collections;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioSource))]

public class bgm : MonoBehaviour
{
	[SerializeField] AudioSource intro, loop;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		intro.Play();
		loop.PlayScheduled(AudioSettings.dspTime + intro.clip.length);
	}
}
