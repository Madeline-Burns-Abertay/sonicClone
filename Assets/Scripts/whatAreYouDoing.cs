using UnityEngine;
using UnityEngine.InputSystem;
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
	private InputAction move, crouch, jump;
	[SerializeField] private LayerMask ground;
	[SerializeField] private Transform groundCheckPoint;
	[SerializeField] private float groundCheckRadius;
	[SerializeField] private float stickForce;
	[SerializeField] private float spindashSpeedIncrement, spindashSpeedCap;
	[SerializeField] private float springLaunchVelocity;

	public float inputX;
	private const float EPSILON = 0.01f;
	private enum State
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
	}

	private void FixedUpdate()
	{
		//ground check (SKIP IFF DEAD)
		grounded = currentState != State.Dead && Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, ground);
		//run up slopes
		if (grounded) rb.gravityScale = 0f;
		else rb.gravityScale = 1f;
		
		inputX = move.ReadValue<float>();
		if (grounded && crouch.inProgress) currentState = State.Crouched;
		//movement with groun stick
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
		if (Mathf.Abs(rb.linearVelocityX) > EPSILON && currentState != State.Hurt) 
			transform.localScale = new Vector3(Mathf.Sign(rb.linearVelocityX), 1) / 2; // half because initial scale is 
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
		if (hit.collider != null && grounded) transform.rotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
		else if (!grounded) transform.rotation = Quaternion.identity;
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
			case State.Spinning:
			case State.FinishedLevel:
				sprite.sprite = spriteAtlas.GetSprite("Circle");
				break;
			case State.Crouched:
				sprite.sprite = spriteAtlas.GetSprite("Circle_Crouch");
				break;
			case State.Spindash:
				sprite.sprite = spriteAtlas.GetSprite("Circle_Spindash");
				break;
			case State.Hurt:
				sprite.sprite = spriteAtlas.GetSprite("Circle_Ouch");
				break;
			case State.Dead:
				sprite.sprite = spriteAtlas.GetSprite("Circle_Dead");
				break;
			default:
				Debug.Assert(false, "wtf");
				break;
		}
		previousState = currentState;
	}

	public int getLives() { return lives; }
}