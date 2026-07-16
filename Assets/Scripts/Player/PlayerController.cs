using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.U2D;

[RequireComponent(typeof(PlayerScore))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(PlayerScore))]
public class PlayerController : MonoBehaviour
{
	[SerializeField] private float speed;
	[SerializeField] private float jumpForce;
	private Rigidbody2D rb;
	private SpriteRenderer sprite;
	[SerializeField] private List<Sprite> sprites;
	private InputAction move, crouch, jump;

	// ground stuff
	[SerializeField] private bool grounded = false, wasGrounded = false;
	[SerializeField] private LayerMask ground;
	[SerializeField] private Transform groundCheckPoint;
	[SerializeField] private float groundCheckRadius;
	[SerializeField] private float stickForce;
	// [SerializeField] private float springLaunchVelocity;
	[SerializeField, Range(0.5f, 2f)] private float size;

	// stuff involving pain/death
	[SerializeField] private float hurtKnockback, deathJumpMultiplier, invincibilityDuration;
	private float invincibilityTimer;
	[SerializeField] private GameObject RingPrefab;
	[SerializeField, Range(10f, 20f)] private float ringScatterRange;
	[SerializeField] private Camera cam;
	private System.Func<bool> fellOffTheScreen;

	private float inputX;
	public enum State
	{
		Normal,
		Crouched,
		Spindash,
		Spinning,
		Hurt,
		Dead,
		FinishedLevel
	}
	private State currentState, previousState;
	// spindash
	private bool isChargingSpindash;
	private float spindashCharge;
	[SerializeField] private float spindashIncrement, spindashCap;

	private PlayerScore score;

	private static int lives = 0;
	[SerializeField] private int startingLives = 3;

	private AudioSource sfxSource;
	[SerializeField] private List<AudioClip> sfxList; // same order as sonic 2
	private enum SFXNames // if only the inspector allowed me to have a dictionary in there... oh well. this will do for now
	{
		Jump,
		Hurt,
		SpindashRelease,
		Spin,
		RingLoss,
		SpindashCharge
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		sprite = GetComponent<SpriteRenderer>();
		move = InputSystem.actions.FindAction("Move");
		crouch = InputSystem.actions.FindAction("Crouch or Spin");
		crouch.Enable();
		jump = InputSystem.actions.FindAction("Jump");
		jump.Enable();
		currentState = previousState = State.Normal;
		spindashCharge = 0f;

		score = GetComponent<PlayerScore>();
		if (lives == 0) lives = startingLives;

		transform.localScale = Vector3.one * size;

		fellOffTheScreen = () => cam.WorldToViewportPoint(transform.position + size * Vector3.up).y < 0;

		sfxSource = GetComponent<AudioSource>();
	}

	private void FixedUpdate()
	{
		//ground check (SKIP IFF DEAD)
		grounded = currentState != State.Dead && Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, ground);
		//run up slopes
		if (grounded && currentState != State.Dead) rb.gravityScale = 0f;
		else rb.gravityScale = 1f;

		inputX = move.ReadValue<float>();
		if (grounded && currentState != State.Spindash && crouch.inProgress)
		{
			if (rb.linearVelocity.magnitude >= Consts.EPSILON)
			{
				playSFX(SFXNames.Spin);
				currentState = State.Spinning;
			}
			else currentState = State.Crouched;
		}
		//movement with ground stick
		switch (currentState)
		{
			case State.Normal:
			case State.Spinning:
				if (grounded) { rb.AddForce(inputX * speed * transform.right - transform.up * stickForce); }
				else { rb.AddForce(0.4f * inputX * speed * transform.right); }
				break;
			default:
				break;
		}
		// direction
		if (Mathf.Abs(rb.linearVelocityX) > Consts.EPSILON && currentState != State.Hurt)
			transform.localScale = new Vector3(Mathf.Sign(rb.linearVelocityX), 1) * size;
		if (rb.linearVelocity.magnitude < Consts.EPSILON) rb.linearVelocity = Vector2.zero;
		// end pain
		if (grounded && !wasGrounded && currentState == State.Hurt)
		{
			currentState = State.Normal;
			rb.linearVelocity = Vector2.zero;
			StartCoroutine(InvincibilityFlicker());
		}
	}

	private IEnumerator InvincibilityFlicker()
	{
		invincibilityTimer = invincibilityDuration;
		Debug.Log("started invincibility flicker");
		while (invincibilityTimer > Consts.EPSILON)
		{
			sprite.enabled = !sprite.enabled;
			yield return new WaitForEndOfFrame();
			invincibilityTimer -= Time.deltaTime;
		}
		sprite.enabled = true;
		Debug.Log("ended invincibility flicker");
	}

	private void Update()
	{
		if (grounded)
		{
			//jump
			if (jump.WasPressedThisFrame())
			{
				if (currentState != State.Crouched && !isChargingSpindash)
				{
					rb.AddForce(jumpForce * transform.up, ForceMode2D.Impulse); // jump normally, taking floor angle into account
					currentState = State.Spinning; // allow player to kill enemies
					playSFX(SFXNames.Jump);
				}
				else
				{
					isChargingSpindash = true; // charge spindash
					spindashCharge = Mathf.Min(spindashCharge + spindashIncrement, spindashCap);
					currentState = State.Spindash;
					playSFX(SFXNames.SpindashCharge);
				}
			}
		}
		if (crouch.WasReleasedThisFrame()) // release spindash
		{
			if (isChargingSpindash)
			{
				currentState = State.Spinning;
				rb.AddForce(Mathf.Sign(transform.localScale.x) * spindashCharge * transform.right);
				spindashCharge = 0f;
				isChargingSpindash = false;
				playSFX(SFXNames.SpindashRelease);
			}
		}
		
		if (currentState == State.Spinning && ((grounded && !wasGrounded) || rb.linearVelocity.magnitude <= Consts.EPSILON))
		{
			currentState = (crouch.inProgress ? State.Crouched : State.Normal);
		}

		//rotate around ground
		RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, 1, ground);
		if (hit.collider != null && grounded && Mathf.Abs(inputX) >= Consts.EPSILON) 
			transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
		else if (!grounded || Mathf.Abs(inputX) < Consts.EPSILON) transform.rotation = Quaternion.identity;
		// time limit
		if (score.isOutOfTime()) StartCoroutine(Die());
	}
	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (isPlaying() || invincibilityTimer <= Consts.EPSILON)
		{
			if (collision.gameObject.CompareTag("Enemy"))
			{
				if (!canKillEnemies())
				{
					hurt();
				}
			}
			if (collision.gameObject.CompareTag("Hazard"))
			{
				hurt();
			}
		}
		if (collision.gameObject.CompareTag("Ring") && isPlaying() && currentState != State.Hurt) // don't let the player collect rings they literally just dropped. probably should've just done this from the beginning
		{
			score.collectRing();
			if (score.getRings() % 100 == 0 && lives < 99) // why lives < 99? because 2 digits for life display
			{
				lives++;
			}
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (isPlaying())
		{
			if (collision.CompareTag("End Sign"))
			{
				StartCoroutine(EndLevel());
			}

			if (collision.CompareTag("Projectile") && invincibilityTimer <= Consts.EPSILON)
			{
				hurt();
			}
		}
	}

	private void hurt()
	{
		if (score.getRings() > 0) // don't kill the player if they have at least one ring
		{
			currentState = State.Hurt;
			playSFX(SFXNames.RingLoss);
			rb.linearVelocity = Vector2.zero;
			rb.AddForce(new Vector3(-Mathf.Sign(transform.localScale.x) * hurtKnockback, hurtKnockback), ForceMode2D.Impulse);
			Vector2 ringScatterForce;
			for (int i = 0; i < score.getRings(); i++)
			{
				GameObject droppedRing = Instantiate(RingPrefab, transform.position, Quaternion.identity);
				// scatter the dropped rings
				Rigidbody2D ringRB = droppedRing.GetComponent<Rigidbody2D>();
				ringScatterForce = UnityEngine.Random.insideUnitCircle * ringScatterRange;
				ringScatterForce = new Vector2(ringScatterForce.x, Mathf.Abs(ringScatterForce.y));
				ringRB.AddForce(ringScatterForce, ForceMode2D.Impulse);
				ringRB.gravityScale = 1f;
			}
			score.resetRings();
		}
		else StartCoroutine(Die());
	}

	private void playSFX(SFXNames name)
	{
		int index = Convert.ToInt32(name);
		sfxSource.clip = sfxList[index];
		sfxSource.Play();
	}
	private void LateUpdate()
	{
		// sprite state machine
		switch (currentState)
		{
			case State.Normal:
			case State.FinishedLevel:
				sprite.sprite = sprites[0]; // normal
				break;
			case State.Spinning:
				sprite.sprite = sprites[5]; // should be the spinning sprite
				break;
			case State.Crouched:
				sprite.sprite = sprites[1]; // crouch
				break;
			case State.Spindash:
				sprite.sprite = sprites[2]; // spindash
				break;
			case State.Hurt:
				sprite.sprite = sprites[3]; // hurt
				break;
			case State.Dead:
				sprite.sprite = sprites[4]; // dead
				break;
			default:
				Debug.Assert(false, $"invalid state somehow - {Consts.ERROR_MESSAGE}");
				break;
		}
		previousState = currentState;
		wasGrounded = grounded;
	}

	public int getLives() { return lives; }

	private IEnumerator Die()
	{
		playSFX(SFXNames.Hurt);
		if (currentState != State.Dead)
		{
			Debug.Log("dead");
			currentState = State.Dead;
			GetComponent<Collider2D>().enabled = false;
			rb.linearVelocity = Vector2.zero;
			rb.AddForce(jumpForce * deathJumpMultiplier * Vector2.up);
			yield return new WaitUntil(fellOffTheScreen);
			yield return new WaitForSeconds(1);
			if (lives > 1)
			{
				lives--;
				SceneManager.LoadScene(SceneManager.GetActiveScene().name);
			}
			else SceneManager.LoadScene("GameOver");
		}
	}
	private void OnDrawGizmos()
	{
		Gizmos.color = (grounded ? Color.green : Color.red);
		Gizmos.DrawWireSphere(groundCheckPoint.transform.position, groundCheckRadius);

	}

	public bool canKillEnemies() {  return currentState == State.Spinning || currentState == State.Spindash; }
	private bool isPlaying() { return currentState != State.Dead && currentState != State.FinishedLevel; }

	private IEnumerator EndLevel()
	{
		Debug.Log("Reached End Sign");
		currentState = State.FinishedLevel;
		yield return new WaitForSeconds(5f);
		SceneManager.LoadScene("ThanksForPlaying"); // only one level - no point not hardcoding it yet
	}
}