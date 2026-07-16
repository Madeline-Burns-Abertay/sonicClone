using System;
using System.Collections;
using UnityEngine;

public class Ring : MonoBehaviour
{
	[SerializeField] private float lifetime;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
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

	// Update is called once per frame
	void Update()
	{
		
	}
}
