using System.Collections;
using System.Threading;
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
	bool isGrounded, isCurledUp, isDead;
	uint score, rings, time, lives;
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
		isDead = false;
		rings = 0;
		score = 0;
		time = 0;
		lives = 3;
		StartCoroutine(IncrementTimer());
	}

	// Update is called once per frame
	void Update()
	{

		walkingHitbox.enabled = !isCurledUp && !isDead;
		rollingHitbox.enabled = isCurledUp && !isDead;
		float horizontal = move.ReadValue<float>();
		if (Mathf.Abs(horizontal) > 0)
		{
			rb.AddForce(new Vector2(horizontal, 0) * speed);
		} else
		{
			rb.AddForce(new Vector2(-rb.linearVelocityX, 0) * speed);
		}
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
			if (rings < 999) { rings++; }
			if (rings % 100 == 0) { lives++; }
			Destroy(collision.gameObject);
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
		isDead = true;
		rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
		yield return new WaitForSeconds(2f);
		if (lives > 0)
		{
			lives--;
		}
	}
}
