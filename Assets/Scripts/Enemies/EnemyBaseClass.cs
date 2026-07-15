using TMPro;
using Unity.Cinemachine;
using UnityEngine;

[RequireComponent(typeof(Collider2D), typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
	protected Camera cam;
	protected bool active;

	protected Vector3 initialPos;
	[SerializeField] protected Vector3 viewPos;
	protected Collider2D hitbox;
	protected SpriteRenderer spriteRenderer;
	protected Rigidbody2D rb;
	[SerializeField] protected int pointValue = 100;

	[SerializeField] protected bool debug, spawnWasVisible;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	protected virtual void Start()
	{
		cam = Camera.main; // having to set it from the inspector every time was getting annoying 
		initialPos = transform.position;
		spawnWasVisible = true;
		hitbox = GetComponent<Collider2D>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		rb = GetComponent<Rigidbody2D>();
	}

	protected virtual void init()
	{

	}
	
	// Update is called once per frame
	protected void Update()
	{
        if (!isVisible(transform.position))
		{
			active = false;
			transform.position = initialPos;
		}
		else
		{
			if (!spawnWasVisible)
			{
				active = true;
				init();
			}
			enemyBehaviour();
		}
	}

	protected void LateUpdate()
    {
        spriteRenderer.enabled = hitbox.enabled = active;
		spawnWasVisible = isVisible(initialPos);
    }

	protected void OnCollisionEnter2D(Collision2D collision)
	{
		GameObject other = collision.gameObject;
		if (other.CompareTag("Player"))
		{
			if (other.GetComponent<PlayerController>().canKillEnemies())
			{
				Destroy(gameObject);
				other.GetComponent<PlayerScore>().addScore(pointValue);
			}
		}
	}

	protected virtual void enemyBehaviour()
	{

	}

	protected bool isVisible(Vector3 pos)
	{
		viewPos = cam.WorldToViewportPoint(pos);
		bool vertical   = -Consts.OFF_SCREEN_LOAD_DIST < viewPos.y && viewPos.y < 1f + Consts.OFF_SCREEN_LOAD_DIST;
		bool horizontal = -Consts.OFF_SCREEN_LOAD_DIST < viewPos.x && viewPos.x < 1f + Consts.OFF_SCREEN_LOAD_DIST;
		return vertical && horizontal;
	}
}
