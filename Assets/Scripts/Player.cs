using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
	Rigidbody2D rb;
	BoxCollider2D walkingHitbox;
	CircleCollider2D rollingHitbox;
	InputAction move, jump, crouch, look;
	[SerializeField] float jumpForce, speed = 1.0f;
	bool isGrounded, isCurledUp;
	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
		walkingHitbox = GetComponent<BoxCollider2D>();
		rollingHitbox = GetComponent<CircleCollider2D>();
		move = InputSystem.actions.FindAction("Move");
		jump = InputSystem.actions.FindAction("Jump");
		isGrounded = true;
		isCurledUp = false;
	}

	// Update is called once per frame
	void Update()
	{
		walkingHitbox.enabled = !isCurledUp;
		rollingHitbox.enabled = isCurledUp;
		float horizontal = move.ReadValue<float>();
		rb.AddForce(new Vector2(horizontal, 0) * speed);
		if (jump.IsInProgress() && isGrounded)
		{
			Debug.Log("jumped");
			isGrounded = false;
			isCurledUp = true;
			rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
		}
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		isGrounded = true;
		isCurledUp = false;
	}
}
