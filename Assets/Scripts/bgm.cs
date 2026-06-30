using UnityEngine;
using System.Collections;
[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(AudioSource))]

public class bgm : MonoBehaviour
{
	[SerializeField] AudioSource intro, loop; // most straightforward way i could think of. requires splitting the track into two files but meh
	
	void Start()
	{
		intro.Play();
		loop.PlayScheduled(AudioSettings.dspTime + intro.clip.length);
	}
}
