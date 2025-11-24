using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	Rigidbody2D rb;
	BoxCollider2D walkingHitbox;
	CircleCollider2D rollingHitbox;
	InputAction move, jumpInput, crouch, look;
	[SerializeField] float jumpForce, speed = 1.0f;
	bool isGrounded, isCurledUp;
	int score, rings, time, lives;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		walkingHitbox = GetComponent<BoxCollider2D>();
		rollingHitbox = GetComponent<CircleCollider2D>();
		move = InputSystem.actions.FindAction("Move");
		jumpInput = InputSystem.actions.FindAction("Jump");
		isGrounded = true;
		isCurledUp = false;
		rings = 0;
		score = 0;
		time = 0;
		lives = 3;
	}

	// Update is called once per frame
	void Update()
	{

		walkingHitbox.enabled = !isCurledUp;
		rollingHitbox.enabled = isCurledUp;
		float horizontal = move.ReadValue<float>();
		rb.AddForce(new Vector2(horizontal, 0) * speed);
		if (jumpInput.WasPressedThisFrame() && isGrounded) // todo: look at https://gmtk.itch.io/platformer-toolkit/devlog/395523/behind-the-code
        {
			jump();
			//Debug.Log("jumped");
			
		}
	}

	void jump()
	{
        isGrounded = false;
        isCurledUp = true;
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }
	private void OnCollisionEnter2D(Collision2D collision)
	{
		isGrounded = true;
		isCurledUp = false;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (collision.CompareTag("Ring"))
		{
			rings++;
			Destroy(collision.gameObject);
		}
	}

	public void resetScore() {
		score = 0;
	}

	public string getRings() { return $"{rings}"; }
}
