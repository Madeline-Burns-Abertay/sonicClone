using System;
using System.Collections;
using UnityEngine;

public class Ring : MonoBehaviour
{
	[SerializeField] private float lifetime;
	private AudioSource sfxSource;
	[SerializeField] private AudioClip collectSFX;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		sfxSource = GetComponent<AudioSource>();
		sfxSource.clip = collectSFX;
		if (GetComponent<Rigidbody2D>().gravityScale > 0f) 
		{
			StartCoroutine(Disappear());
		}
	}

	private IEnumerator Disappear()
	{
		yield return new WaitForSeconds(lifetime);
		Destroy(gameObject);
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Player"))
		{
			StartCoroutine(Collect());
		}
	}

	private IEnumerator Collect()
	{
        sfxSource.Play();
		GetComponent<Collider2D>().enabled = false;
		GetComponent<SpriteRenderer>().enabled = false;
		yield return new WaitUntil( () => { return !sfxSource.isPlaying; } );
        Destroy(gameObject);
    }
}
