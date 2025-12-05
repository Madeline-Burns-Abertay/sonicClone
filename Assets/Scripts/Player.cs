using System.Collections;
using System.Threading;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
	Rigidbody2D rb;
	BoxCollider2D walkingHitbox;
	CircleCollider2D rollingHitbox;
	InputAction move, jumpInput, crouch, look;
	[SerializeField] float jumpForce, speed, spindashSpeedCap;
	float spindashSpeed;
	enum State
	{
		Idle,
		Running,
		Jumped,
		Crouched,
		Spindash,
		Rolling,
		Hurt,
		Dead
	}
	State state;
	bool currentlyPressingJump;
	uint score, rings, time, lives;
	// Start is called once before the first execution of Update after the MonoBehaviour is created

	public void OnJump(InputAction.CallbackContext context)
	{
		if (context.started)
		{
			state = State.Jumped;
			currentlyPressingJump = true;
		}
		if (context.canceled)
		{
			currentlyPressingJump = false;
		}
	}

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		walkingHitbox = GetComponent<BoxCollider2D>();
		rollingHitbox = GetComponent<CircleCollider2D>();
		move = InputSystem.actions.FindAction("Move");
		jumpInput = InputSystem.actions.FindAction("Jump");
		crouch = InputSystem.actions.FindAction("Crouch/Spin");
		state = State.Idle;
		rings = 0;
		score = 0;
		time = 0;
		lives = 3;
		StartCoroutine(IncrementTimer());
	}

	void stateTransition(State newState, bool condition)
	{
		if (condition) state = newState;
	}

	// Update is called once per frame
	void Update()
	{
		float input = move.ReadValue<float>();
		switch (state)
		{
		case State.Idle:
			walkingHitbox.enabled = true;
			rollingHitbox.enabled = false;
			stateTransition(State.Running, !Mathf.Approximately(input, 0));
			stateTransition(State.Crouched, crouch.WasPressedThisFrame());
			
			break;
		case State.Running:
			walkingHitbox.enabled = true;
			rollingHitbox.enabled = false;
			if (!Mathf.Approximately(input, 0))
			{
				transform.localScale = new Vector3(Mathf.Sign(input), 1);
			}
			if (Mathf.Abs(input) > 0) 
			{
				rb.AddForce(new Vector2(input, 0) * speed);
			}
			else
			{
				rb.AddForce(new Vector2(-rb.linearVelocityX, 0));
			}
			stateTransition(State.Idle, Mathf.Approximately(rb.linearVelocityX, 0));
			break;
		case State.Crouched:
			stateTransition(State.Spindash, jumpInput.WasPressedThisFrame());
			stateTransition(State.Idle, crouch.WasReleasedThisFrame());
			break;
		case State.Spindash:
			if (jumpInput.WasPressedThisFrame() && spindashSpeed < spindashSpeedCap)
			{
				spindashSpeed += 5;
			}
			if (crouch.WasReleasedThisFrame())
			{
				rb.AddForce(new Vector2(transform.localScale.x, 0) * spindashSpeed, ForceMode2D.Impulse);
				stateTransition(State.Rolling, true);
			}
			break;
		case State.Jumped:
			break;
		case State.Dead:
			StartCoroutine(Die());
			break;
		default:
			Debug.Assert(false, "This should never happen. If it does, the game's fucked - contact 2501892@abertay.ac.uk immediately.");
			break;
		}
		
		if (Mathf.Abs(input) > 0)
		{
			
		} else
		{
			
		}
/*		if (jumpInput.WasPressedThisFrame() && isGrounded) // todo: look at https://gmtk.itch.io/platformer-toolkit/devlog/395523/behind-the-code
		{
			jump();
			
		}*/
	}

	/*void jump()
	{
		isGrounded = false;
		isCurledUp = true;
		rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
	}*/

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ring"))
		{
			if (rings < 999) { rings++; }
			if (rings % 100 == 0) { lives++; }
			Destroy(collision.gameObject);
		}
		if (collision.CompareTag("Enemy"))
		{
			State newState = (rings > 0 ? State.Hurt : State.Dead);
			stateTransition(newState, state != State.Rolling && state != State.Jumped);
		}
		if (collision.CompareTag("Hazard"))
		{
			State newState = (rings > 0 ? State.Hurt : State.Dead);
			stateTransition(newState, true);
		}
	}

	public void resetScore() {
		score = 0;
	}

	public uint getRings() { return rings; }
	public string getScore() { return $"{score.ToString().PadLeft(7)}"; }
	public string getTime() {
		int mins = Mathf.FloorToInt(time / 60);
		int secs = (int)time % 60;
		return $"{mins}:{secs:00}";
	}
	IEnumerator IncrementTimer()
	{
		while (time < 599)
		{
			yield return new WaitForSeconds(1f);
			time++;
		}
		StartCoroutine(Die());
	}

	IEnumerator Die()
	{
		rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
		yield return new WaitForSeconds(2f);
		lives--;
		if (lives == 0) {
			SceneManager.LoadScene("GameOverScreen");
		}
	}
}
