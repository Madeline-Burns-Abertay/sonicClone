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
public class PlayerController : MonoBehaviour // huh. last i heard the class name had to match the filename, but apparently this works
{
	[SerializeField] private float speed;
	[SerializeField] private float jumpForce;
	private Rigidbody2D rb;
	private SpriteRenderer sprite;
	[SerializeField] private SpriteAtlas spriteAtlas;
	[SerializeField] private List<Sprite> sprites;
	private InputAction move, crouch, jump;
	[SerializeField] private LayerMask ground;
	[SerializeField] private Transform groundCheckPoint;
	[SerializeField] private float groundCheckRadius;
	[SerializeField] private float stickForce;
	[SerializeField] private float spindashSpeedIncrement, spindashSpeedCap;
	// [SerializeField] private float springLaunchVelocity;
	[SerializeField, Range(0.5f, 2f)] private float size;

	// stuff involving death
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
	private bool isChargingSpindash;
	private float spindashCharge;

	[SerializeField] private float spindashIncrement, spindashCap;

	[SerializeField] private bool grounded = false;

	private PlayerScore score;

	private static int lives = 0;
	[SerializeField] private int startingLives = 3;

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
			currentState = rb.linearVelocity.magnitude < Consts.EPSILON ? State.Crouched : State.Spinning;
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
	}
	private void Update()
	{
		if (grounded)
		{
			//jump
			if (jump.WasPressedThisFrame())
			{
				if (currentState != State.Crouched)
				{
					rb.AddForce(jumpForce * transform.up, ForceMode2D.Impulse); // jump normally, taking floor angle into account
					currentState = State.Spinning; // allow player to kill enemies
				}
				else
				{
					isChargingSpindash = true; // charge spindash
					spindashCharge = Mathf.Min(spindashCharge + spindashIncrement, spindashCap);
					currentState = State.Spindash;
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
			}
			else currentState = State.Normal;
		}
		// end pain once back on ground
		if (grounded && currentState == State.Hurt)
		{
			currentState = State.Normal;
		}

		//rotate around ground
		RaycastHit2D hit = Physics2D.Raycast(transform.position, -transform.up, 1, ground);
		if (hit.collider != null && grounded && Mathf.Abs(inputX) >= Consts.EPSILON) 
			transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
		else if (!grounded || Mathf.Abs(inputX) < Consts.EPSILON) transform.rotation = Quaternion.identity;
		// time limit
		if (score.isOutOfTime()) StartCoroutine(Die());
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		//triggers for springs, enemies, ring, etc
		
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
		//Debug.Log($"gravity scale {rb.gravityScale}\nforce acting {rb.totalForce}");
	}

	public int getLives() { return lives; }

	private IEnumerator Die()
	{
		if (currentState != State.Dead)
		{
			currentState = State.Dead;
			GetComponent<Collider2D>().enabled = false;
			rb.linearVelocity = Vector2.zero;
			rb.AddForce(jumpForce * Vector2.up);
            //Debug.Log($"force acting {rb.totalForce}");
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
}