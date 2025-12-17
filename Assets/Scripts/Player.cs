using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{

	private Rigidbody2D rb;
	private BoxCollider2D walkingHitbox;
	private CircleCollider2D rollingHitbox;
	private GroundChecker groundCheck;
	private Vector2 velocity;

	private InputAction move, jumpInput, crouch;
	[SerializeField] private float speed, spindashSpeedIncrement, spindashSpeedCap, hurtKnockback;
	private float spindashSpeed;
	private bool onGround;
	
	[SerializeField] private float jumpHeight = 10f;
	[SerializeField, Range(0.2f, 1.25f)] private float timeToJumpApex = 0.5f;
	[SerializeField, Range(0f, 5f)] private float upwardMovementMultiplier = 1f;
	[SerializeField, Range(1f, 10f)] private float downwardMovementMultiplier = 6.17f;

	[SerializeField, Range(1f, 10f)][Tooltip("Gravity multiplier when you let go of jump")] private float jumpCutOff; // y'know what
																													  // i think this tooltip might actually be helpful
																													  // i'm keeping it
	[SerializeField] public float fallSpeedLimit;

	private float jumpSpeed;
	private float defaultGravityScale;
	private float gravMultiplier;

	private enum State
	{
		Idle,
		Running,
		Jumping,
		Crouched,
		Spindash,
		Rolling,
		Hurt,
		Dead,
		FinishedLevel
	}
	private State state;
	private bool desiredJump, currentlyPressingJump;
	private const float EPSILON = 0.01f; // because holy hell this ball never legally stops moving without this

	private SpriteRenderer spriteRenderer;
	[SerializeField] private Sprite normal, crouchSprite, spindash, ouch, dead;

	[SerializeField] private Camera cam;
	
	[SerializeField] private GameObject RingPrefab;

	[SerializeField] private AudioSource sfxPlayer;
	[SerializeField] private List<AudioClip> sfx; // same order as the original game, as per https://forums.sonicretro.org/threads/sound-test-sfx-listings.28870/#post-692729
												  // ignore the ones that have no obvious analog here (yet!)



	private int score, rings, time;
	private static int lives;
	[SerializeField] private int startingLives = 3;

	[SerializeField] private int timeLimitMins = 10; // i was gonna leave this as a magic number since it's only used once
													 // and never supposed to be anything other than 10 
													 // but i figured i probably should make it a variable anyway
	[SerializeField] private float ringScatterRange = 5f;


    private void playSFX(int index)
	{
		sfxPlayer.clip = sfx[index];
		sfxPlayer.Play();
	}

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			switch (state)
			{
				case State.Crouched:
					spindashSpeed = spindashSpeedIncrement;
					stateTransition(State.Spindash, true, 5); // spin sfx, in this case charging up a spindash
					break;
				case State.Spindash:
					playSFX(5); // second verse, same as the first
					if (spindashSpeed < spindashSpeedCap)
					{
						spindashSpeed += spindashSpeedIncrement;
						Debug.Log($"spindash speed: {spindashSpeed}/{spindashSpeedCap}");
					}
					break;
				default:
					desiredJump = true;
					break;
			}
			currentlyPressingJump = true;
		}
		if (context.canceled)
		{
			currentlyPressingJump = false;
		}
	}

	private void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		defaultGravityScale = rb.gravityScale;
		walkingHitbox = GetComponent<BoxCollider2D>();
		rollingHitbox = GetComponent<CircleCollider2D>();
		groundCheck = GetComponent<GroundChecker>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		move = InputSystem.actions.FindAction("Move");
		jumpInput = InputSystem.actions.FindAction("Jump");
		jumpInput.Enable();
		jumpInput.started += OnJump;
		jumpInput.canceled += OnJump;
		crouch = InputSystem.actions.FindAction("Crouch or Spin");
		state = State.Idle;
		rings = 0;
		score = 0;
		time = 0;
		if (lives == 0)
		{
			lives = startingLives;
		}
		sfxPlayer = GetComponent<AudioSource>();
		StartCoroutine(IncrementTimer());
	}

	private void stateTransition(State newState, bool condition)
	{
		if (condition)
		{
			state = newState;
			Debug.Log($"new state: {newState}");
		}
	}

	private void stateTransition(State newState, bool condition, int sfxIndex) // sound effect to play on state transition
	{
		if (condition)
		{
			playSFX(sfxIndex);
			state = newState;
			Debug.Log($"new state: {newState}");
		}
	}

	// Update is called once per frame
	private void Update()
	{
		spriteRenderer.sprite = normal;
		setPhysics();
		onGround = groundCheck.GetOnGround();
		//Debug.Log($"current state: {state}");
		bool fellOffTheScreen = cam.WorldToViewportPoint(transform.position).y < 0 && state != State.Dead; // don't want to kill the player twice

		stateTransition(State.Dead, fellOffTheScreen && state != State.FinishedLevel); // impossible in the only level that currently exists, but it doesn't hurt
																					   // to have it ready in case i ever decide to expand on this
		float input = move.ReadValue<float>();
		switch (state)
		{
			case State.Idle:
				walkingHitbox.enabled = true;
				rollingHitbox.enabled = false;
				transform.rotation = Quaternion.identity;
				stateTransition(State.Running, !Mathf.Approximately(input, 0));
				stateTransition(State.Crouched, crouch.WasPressedThisFrame());
				break;

			case State.Running:
				walkingHitbox.enabled = true;
				rollingHitbox.enabled = false;
				if (!Mathf.Approximately(input, 0))
				{
					transform.localScale = new Vector3(Mathf.Sign(input) * Mathf.Abs(transform.localScale.x), transform.localScale.y);
				}
				if (Mathf.Abs(input) > 0) 
				{
					rb.AddForceX(input * speed);
				}
				else
				{
					rb.AddForce(new Vector2(-rb.linearVelocityX, 0));
				}
				stateTransition(State.Idle, Mathf.Approximately(rb.linearVelocityX, 0));
				stateTransition(State.Rolling, crouch.WasPressedThisFrame(), 5); // spin sfx
				break;

			case State.Crouched:
				transform.rotation = Quaternion.identity;
				spriteRenderer.sprite = crouchSprite;
				stateTransition(State.Idle, crouch.WasReleasedThisFrame());
				break;

			case State.Spindash:
				spriteRenderer.sprite = spindash;
				if (crouch.WasReleasedThisFrame())
				{
					rb.AddForce(new Vector2(transform.localScale.x, 0) * spindashSpeed, ForceMode2D.Impulse);
					stateTransition(State.Rolling, true, 4); // spindash release sfx
				}
				break;

			case State.Rolling:
				rollingHitbox.enabled = true;
				walkingHitbox.enabled = false;
				bool pressingBackwards = Mathf.Sign(input) != Mathf.Sign(rb.linearVelocityX) && input != 0;

				rb.AddForceX((pressingBackwards ? (input * Mathf.Abs(rb.linearVelocityX)) : 0));
				if (Mathf.Approximately(rb.linearVelocity.magnitude, 0))
				{
					stateTransition(State.Crouched, crouch.inProgress);
					stateTransition(State.Idle, !crouch.inProgress);
				}
				break;
			case State.Dead:
				rb.linearVelocity = Vector2.zero;
				StartCoroutine(Die());
				break;

			case State.FinishedLevel:
				rb.AddForce(Vector2.right);
				StartCoroutine(EndLevel());
				break;

			case State.Jumping:
				if (!Mathf.Approximately(input, 0))
				{
					transform.localScale = new Vector3(Mathf.Sign(input) * transform.localScale.x, transform.localScale.y);
				}
				if (Mathf.Abs(input) > 0)
				{
					rb.AddForceX(input * speed);
				}
				else
				{
					rb.AddForceX(-rb.linearVelocityX);
				}
				break;
			case State.Hurt:
				stateTransition(State.Idle, onGround);
				break;

			default:
				Debug.Assert(false, "This should never happen. If it does, the game's broken - contact 2501892@abertay.ac.uk immediately.");
				break;
		}
		if (Mathf.Abs(rb.linearVelocityX) < EPSILON) rb.linearVelocityX = 0;
	}

	private void setPhysics()
	{
		//Determine the character's gravity scale, using the stats provided. Multiply it by a gravMultiplier, used later
		float newGravity = (-2 * jumpHeight) / (timeToJumpApex * timeToJumpApex);
		rb.gravityScale = (newGravity / Physics2D.gravity.y) * gravMultiplier;
	}

	private void FixedUpdate()
	{
		//Get velocity from [placeholder]'s Rigidbody 
		velocity = rb.linearVelocity;

		//Keep trying to do a jump, for as long as desiredJump is true
		if (desiredJump)
		{
			DoAJump();
			rb.linearVelocity = velocity;

			//Skip gravity calculations this frame, so currentlyJumping doesn't turn off
			//This makes sure you can't do the coyote time double jump bug
			return;
		}

		calculateGravity();
	}

	private void calculateGravity()
	{
		//We change the character's gravity based on [its] Y direction

		//If [placeholder] is going up...
		if (rb.linearVelocity.y > 0.01f)
		{
			//Apply upward multiplier if player is rising and holding jump
			if (currentlyPressingJump && state == State.Jumping)
			{
				gravMultiplier = upwardMovementMultiplier;
			}
			//But apply a special downward multiplier if the player lets go of jump
			else
			{
				gravMultiplier = jumpCutOff;
			}
		}

		//Else if going down...
		else if (rb.linearVelocity.y < -0.01f)
		{
				// apply the downward gravity multiplier as [placeholder] comes back to Earth
				gravMultiplier = downwardMovementMultiplier;
		}
		//Else not moving vertically at all
		else
		{
			State newState = (Mathf.Abs(rb.linearVelocityX) > 0 ? State.Running : State.Idle);
			stateTransition(newState, onGround && (state == State.Jumping || state == State.Hurt));

			gravMultiplier = defaultGravityScale;
		}
	}

	private void DoAJump()
	{
		

		//Create the jump, provided we are on the ground
		if (onGround)
		{
			playSFX(0); // jump sfx
			desiredJump = false;

			//Determine the power of the jump, based on our gravity and stats
			jumpSpeed = Mathf.Sqrt(-2f * Physics2D.gravity.y * rb.gravityScale * jumpHeight);

			//If [placeholder] is moving up or down when [it] jumps (such as when doing a double jump), change the jumpSpeed;
			//This will ensure the jump is the exact same strength, no matter your velocity.
			if (velocity.y > 0f)
			{
				jumpSpeed = Mathf.Max(jumpSpeed - velocity.y, 0f);
			}
			else if (velocity.y < 0f)
			{
				jumpSpeed += Mathf.Abs(rb.linearVelocity.y);
			}

			//Apply the new jumpSpeed to the velocity. It will be sent to the Rigidbody in FixedUpdate;
			velocity.y += jumpSpeed;
			state = State.Jumping;
		}

		//... we don't have a jump buffer, [so] turn off desiredJump immediately after hitting jumping
		desiredJump = false;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Enemy"))
		{
			if (state == State.Rolling || state == State.Jumping)
			{
				playSFX(6); // enemy pop sfx
				Destroy(collision.gameObject);
				addScore(10);
			}
			else
			{
				hurt(1); // normal hurt sfx
			}
		}
		if (collision.gameObject.CompareTag("Hazard"))
		{
			hurt(2); // spike hurt sfx
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ring"))
		{

			playSFX((rings % 2 == 0 ? 3 : 8)); // ring pickup sfx, 3 in the right ear and 8 in the left
			if (rings < 999) // 3 digits for ring display
			{
				rings++;
			}
			if (rings % 100 == 0 && lives < 99) // why lives < 99? because 2 digits for life display
			{
				lives++;
			}
			Destroy(collision.gameObject);
		}

		if (collision.CompareTag("End Sign"))
		{
			Debug.Log("Reached End Sign");
			stateTransition(State.FinishedLevel, true, 9); // end signpost sfx
			collision.GetComponent<SpriteRenderer>().color = new Color(0.35f, 0.64f, 0.93f);
		}
		
	}

	private void hurt(int sfxIndex)
	{
		playSFX(sfxIndex);
		State newState = (rings > 0 ? State.Hurt : State.Dead);
		stateTransition(newState, true);
		if (state == State.Hurt) // don't want any of this to happen if the player dies
		{
			playSFX(7); // ring loss sfx
			rb.linearVelocity = Vector2.zero;
			rb.AddForce(new Vector3(-Mathf.Sign(transform.localScale.x) * hurtKnockback, hurtKnockback), ForceMode2D.Impulse);
			GameObject droppedRing;
			Rigidbody2D ringRB;
			Vector2 ringScatterForce;
			for (int i = 0; i < rings; i++)
			{
				droppedRing = Instantiate(RingPrefab, transform);
				// scatter the dropped rings
				ringRB = droppedRing.GetComponent<Rigidbody2D>();
				ringScatterForce = Random.insideUnitCircle * ringScatterRange;
				ringScatterForce = new Vector2(ringScatterForce.x, Mathf.Abs(ringScatterForce.y));
				ringRB.AddForce(ringScatterForce, ForceMode2D.Impulse);
				ringRB.gravityScale = rb.gravityScale;
			}
			rings = 0;
		}
	}

	public void resetScore() {
		score = 0;
	}

	public void addScore(int points)
	{
		score = Mathf.Min(score + (points * 10), 9999990); // 7 digit score display, the last of which is always zero
	}

	public int getRings() { return rings; }
	public int getLives() { return lives; }
	public string getScore() { return $"{score.ToString().PadLeft(7)}"; }
	public string getTime() {
		int mins = Mathf.FloorToInt(time / 60);
		int secs = (int)time % 60;
		return $"{mins}:{secs:00}";
	}
	private IEnumerator IncrementTimer()
	{
		while (time < timeLimitMins * 60 - 1)
		{
			yield return new WaitForSeconds(1f);
			time++;
		}
		StartCoroutine(Die());
	}

	private IEnumerator EndLevel()
	{
		yield return new WaitForSeconds(5f);
		SceneManager.LoadScene("ThanksForPlaying"); // only one level - no point not hardcoding it yet
	}

	private IEnumerator Die()
	{
		walkingHitbox.enabled = false;
		rollingHitbox.enabled = false;
		rb.linearVelocity = Vector2.zero;
		rb.AddForce(jumpHeight * Vector2.up, ForceMode2D.Impulse);
		yield return new WaitForSeconds(2f);
		lives--;
		if (lives == 0) {
			SceneManager.LoadScene("GameOver");
		}
		else
		{
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}
	}
}
