using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class FlyingEnemy : Enemy
{
	[SerializeField, Range(-10f, 0f)] private float startingOffset;
	[SerializeField, Range(0f, 0.5f), Tooltip("the range for where the enemy can stop to fire. note: viewport coords; symmetric")]
		private float stopRange;
	private float screenStopPoint;
	[SerializeField] private float worldStopPoint; // where it WILL stop to fire
	
	private bool firing, fired;
	[SerializeField, Range(1f, 5f)] private float stopDurationSeconds;
	[SerializeField] private float speed;
	[SerializeField] private GameObject projectile;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	protected override void Start()
	{
		base.Start();
		rb.bodyType = RigidbodyType2D.Kinematic;
		//init();
	}

	protected override void init()
	{
		transform.position = new Vector2(cam.ViewportToWorldPoint(Vector3.zero).x, initialPos.y);
		screenStopPoint = Random.Range(-stopRange, stopRange) + 0.5f; // need to add 0.5f so that the middle of the screen = the middle of the range
		worldStopPoint = cam.ViewportToWorldPoint(new Vector3(screenStopPoint, 0)).x;
		fired = false;
	}

	// Update is called once per frame
	protected override void enemyBehaviour()
	{
		if (!firing) rb.linearVelocityX = speed;
		if (!fired && transform.position.x >= worldStopPoint) StartCoroutine(fire());
		
	}
	private IEnumerator fire()
	{
		Debug.Log("firing");
		firing = true;
		rb.linearVelocityX = 0f;
		yield return new WaitForSeconds(stopDurationSeconds / 2);
		if (!fired)
		{
			Instantiate(projectile, transform.position, Quaternion.identity);
			fired = true;
		}
		yield return new WaitForSeconds(stopDurationSeconds / 2);
		firing = false;
	}
}
